const path = require('path');
const MiniCssExtractPlugin = require('mini-css-extract-plugin');
const { CleanWebpackPlugin } = require('clean-webpack-plugin');
const CopyWebpackPlugin = require('copy-webpack-plugin');
const { WebpackManifestPlugin } = require('webpack-manifest-plugin');

module.exports = {
    mode: 'development',
    entry: {
        site: ['./src/scss/site.scss', './src/js/site.js'],
        validation: './src/js/validation.js'
    },
    output: {
        path: path.resolve(__dirname, 'wwwroot'),
        filename: 'js/[name].js',
        chunkFilename: 'js/[name].chunk.js',
        assetModuleFilename: 'assets/[name][ext]',
        clean: {
            keep: /\.gitkeep/,
        },
        publicPath: '/'
    },
    module: {
        rules: [
            {
                test: /\.js$/,
                exclude: /node_modules/,
                use: {
                    loader: 'babel-loader',
                    options: {
                        presets: [
                            ['@babel/preset-env', {
                                targets: {
                                    browsers: ['> 1%', 'last 2 versions', 'not dead']
                                },
                                modules: false
                            }]
                        ]
                    }
                }
            },
            {
                test: /\.scss$/,
                use: [
                    MiniCssExtractPlugin.loader,
                    {
                        loader: 'css-loader',
                        options: {
                            importLoaders: 3,
                            sourceMap: true
                        }
                    },
                    {
                        loader: 'postcss-loader',
                        options: {
                            postcssOptions: {
                                plugins: [['autoprefixer']]
                            }
                        }
                    },
                    {
                        loader: 'sass-loader',
                        options: {
                            sourceMap: true
                        }
                    }
                ]
            },
            {
                test: /\.css$/,
                use: [
                    MiniCssExtractPlugin.loader,
                    {
                        loader: 'css-loader',
                        options: {
                            importLoaders: 1,
                            sourceMap: true
                        }
                    },
                    {
                        loader: 'postcss-loader',
                        options: {
                            postcssOptions: {
                                plugins: [['autoprefixer']]
                            }
                        }
                    }
                ]
            },
            {
                test: /\.(woff|woff2|eot|ttf|otf)$/,
                type: 'asset/resource',
                generator: {
                    filename: 'fonts/[name][ext]'
                }
            },
            {
                test: /\.(png|jpg|jpeg|gif|svg)$/,
                type: 'asset',
                parser: {
                    dataUrlCondition: {
                        maxSize: 8 * 1024 // 8kb
                    }
                },
                generator: {
                    filename: 'images/[name][ext]'
                }
            }
        ]
    },
    plugins: [
        new CleanWebpackPlugin({
            cleanOnceBeforeBuildPatterns: ['**/*', '!.gitkeep'],
            cleanStaleWebpackAssets: false,
            verbose: true
        }),
        new MiniCssExtractPlugin({
            filename: 'css/[name].css',
            chunkFilename: 'css/[name].chunk.css'
        }),
        new CopyWebpackPlugin({
            patterns: [
                {
                    from: 'src/favicon.ico',
                    to: 'favicon.ico'
                }
            ]        }),
        new WebpackManifestPlugin({
            fileName: 'assets-manifest.json',
            publicPath: '',
            filter: (file) => {
                // Only include JS and CSS files
                return file.name.endsWith('.js') || file.name.endsWith('.css');
            }
        })
    ],
    resolve: {
        extensions: ['.js', '.scss', '.css'],
        alias: {
            '@': path.resolve(__dirname, 'src'),
            '@scss': path.resolve(__dirname, 'src/scss'),
            '@js': path.resolve(__dirname, 'src/js')
        }
    },
    devtool: 'eval-source-map',
    devServer: {
        static: {
            directory: path.join(__dirname, 'wwwroot'),
        },
        compress: true,
        port: 3000,
        hot: true,
        open: true,
        watchFiles: ['src/**/*']
    },
    stats: {
        assets: true,
        children: false,
        chunks: false,
        modules: false,
        builtAt: true,
        timings: true
    }
};
