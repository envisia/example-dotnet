'use strict';

const webpack = require('webpack');
const path = require('path');
const MiniCssExtractPlugin = require("mini-css-extract-plugin");
const CssMinimizerPlugin = require('css-minimizer-webpack-plugin');
const TerserJSPlugin = require('terser-webpack-plugin');
const CopyPlugin = require('copy-webpack-plugin');
const {WebpackManifestPlugin} = require('webpack-manifest-plugin');

const basePath = path.join(__dirname, './');

const port = parseInt(process.env.PORT, 10) || 8083;

module.exports = function createExport(isProduction) {
    const cssLoaders = [
        {loader: MiniCssExtractPlugin.loader},
        {loader: 'css-loader', options: {importLoaders: 1}},
        {loader: 'sass-loader'},
    ];

    const devServer = {
        cache: {
            type: 'filesystem',
            cacheDirectory: path.resolve(__dirname, 'node_modules', '.temp_cache'),
        },
        devServer: {
            writeToDisk: true,
            compress: true,
            port: port,
            contentBase: basePath,
            publicPath: '/',
            inline: false,
            onListening: function (server) {
                // lazy line for dotnet core:
                // https://github.com/dotnet/aspnetcore/blob/master/src/Middleware/SpaServices.Extensions/src/ReactDevelopmentServer/ReactDevelopmentServerMiddleware.cs#L91
                console.log('Starting the development server...\n');
                const port = server.listeningApp.address().port;
                console.log('Listening on port:', port);
            }
        }
    };

    const typescriptRule = {
        test: /\.ts(x?)$/,
        exclude: /node_modules/,
        use: [
            {
                loader: 'ts-loader'
            }
        ]
    };

    const common = {
        context: basePath,
        mode: isProduction ? 'production' : 'development',
        resolve: {
            extensions: ['.jsx', '.js', '.ts', '.tsx']
        }
    };

    if (!isProduction) {
        common['devtool'] = 'eval-cheap-module-source-map';
    }

    const babelLoader = {
        test: /\.jsx$/,
        use: {
            loader: 'babel-loader',
            options: {
                presets: ['@babel/react', ['@babel/preset-env', {'targets': {'browsers': '> 5%'}}]],
                plugins: ['@babel/plugin-syntax-dynamic-import']
            },
        }
    };

    const optimization = {
        minimize: isProduction,
        minimizer: [new TerserJSPlugin({}), new CssMinimizerPlugin()],
    }

    if (!isProduction) {
        optimization.runtimeChunk = {
            name: 'ssr/runtime', // necessary when using multiple entrypoints on the same page
        };
        optimization.splitChunks = {
            cacheGroups: {
                commons: {
                    test: /[\\/]node_modules[\\/](react|react-dom)[\\/]/,
                    name: 'ssr/vendor',
                    chunks: 'all',
                },
            },
        };
    }

    const entry = {
        'components': ['./entry-client.js'],
    }
    if (!isProduction) {
        entry['ssr/components'] = ['./entry-server.js'];
    }

    const web = Object.assign({}, common, {
        name: 'web',
        entry: entry,
        optimization: optimization,
        output: {
            globalObject: 'this',
            path: path.join(__dirname, './build'),
            publicPath: '/dist/',
            filename: isProduction ? '[name].[chunkhash:8].min.js' : '[name].js',
        },
        module: {
            rules: [
                typescriptRule,
                babelLoader,
                {
                    test: /\.(woff(2)?|ttf|eot|svg)(\?v=\d+\.\d+\.\d+)?$/,
                    use: [{
                        loader: 'file-loader',
                        options: {
                            name: '[name].[ext]',
                            outputPath: './fonts'
                        }
                    }]
                },
                {
                    test: /\.(jpe?g|png|gif|svg)$/i,
                    use: [{
                        loader: 'file-loader',
                        options: {
                            name: '[name].[ext]',
                            outputPath: './images',
                            publicPath: '../dist/images/',
                        }
                    }],
                },
                {
                    test: /\.m?js$/,
                    exclude: /(node_modules|bower_components)/,
                    use: [
                        {
                            loader: 'babel-loader',
                            options: {presets: ['@babel/preset-env'], plugins: ['@babel/plugin-syntax-dynamic-import']}
                        }
                    ]
                },
                {
                    test: /\.(sa|sc|c)ss$/,
                    use: cssLoaders,
                }
            ],
        },
        plugins: [
            new MiniCssExtractPlugin({
                // Options similar to the same options in webpackOptions.output
                // both options are optional
                filename: isProduction ? '[name].[chunkhash:8].min.css' : '[name].css'
            }),
            new CopyPlugin({
                patterns: [
                    {from: 'index.html', to: 'index.html', toType: 'file'},
                ]
            }),
            new WebpackManifestPlugin({
                filter: (descriptor) => descriptor.isInitial,
                map: (descriptor) => {
                    if (descriptor.path.startsWith('/dist/')) {
                        const path = descriptor.path.slice(6);
                        return {...descriptor, ...{path: path}};
                    } else {
                        return descriptor;
                    }

                }
            }),
        ]
    }, isProduction ? {} : devServer);

    const ssr = Object.assign({}, common, {
        name: 'ssr',
        entry: {
            components: './entry-server.js'
        },
        output: {
            globalObject: 'this',
            path: path.resolve(__dirname, './build/ssr'),
            filename: '[name].js'
        },
        optimization: {
            runtimeChunk: {
                name: 'runtime', // necessary when using multiple entrypoints on the same page
            },
            splitChunks: {
                cacheGroups: {
                    commons: {
                        test: /[\\/]node_modules[\\/](react|react-dom)[\\/]/,
                        name: 'vendor',
                        chunks: 'all',
                    },
                },
            },
        },
        module: {
            rules: [
                typescriptRule,
                Object.assign({}, babelLoader, {exclude: /node_modules/}),
                {
                    test: /\.(jpe?g|png|gif|svg)$/i,
                    use: [{
                        loader: 'file-loader',
                        options: {
                            name: '[name].[ext]',
                            outputPath: './images',
                            publicPath: '../dist/images/',
                            emitFile: false,
                        }
                    }],
                },
            ]
        }
    }, isProduction ? {} : devServer);

    if (!isProduction) {
        return [web];
    }

    return [web, ssr];
};
