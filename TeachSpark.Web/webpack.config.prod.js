const path = require('path');
const MiniCssExtractPlugin = require('mini-css-extract-plugin');
const CssMinimizerPlugin = require('css-minimizer-webpack-plugin');
const TerserPlugin = require('terser-webpack-plugin');
const { CleanWebpackPlugin } = require('clean-webpack-plugin');
const CopyWebpackPlugin = require('copy-webpack-plugin');
const BundleAnalyzerPlugin = require('webpack-bundle-analyzer').BundleAnalyzerPlugin;
const { WebpackManifestPlugin } = require('webpack-manifest-plugin');

module.exports = {
    mode: 'production',
    entry: {
        // Split vendor libraries from app code
        vendor: ['bootstrap', 'jquery'],
        site: ['./src/scss/site.scss', './src/js/site.js'],
        validation: './src/js/validation.js'
    },
    output: {
        path: path.resolve(__dirname, 'wwwroot'),
        filename: 'js/[name].[contenthash:8].js',
        chunkFilename: 'js/[name].[contenthash:8].chunk.js',
        assetModuleFilename: 'assets/[name].[contenthash:8][ext]',
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
                                modules: false,
                                useBuiltIns: 'usage',
                                corejs: 3
                            }]
                        ],
                        plugins: ['@babel/plugin-syntax-dynamic-import']
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
                            sourceMap: false
                        }
                    },                    {
                        loader: 'postcss-loader',
                        options: {
                            postcssOptions: {
                                plugins: [
                                    ['autoprefixer', {
                                        overrideBrowserslist: ['> 1%', 'last 2 versions', 'not dead', 'not ie 11'],
                                        grid: 'autoplace',
                                        flexbox: 'no-2009'
                                    }],
                                    ['postcss-preset-env', {
                                        stage: 2,
                                        features: {
                                            'nesting-rules': true,
                                            'custom-properties': true
                                        },
                                        autoprefixer: false
                                    }]
                                ]
                            },
                            sourceMap: false
                        }
                    },
                    {
                        loader: 'sass-loader',
                        options: {
                            sourceMap: false,
                            sassOptions: {
                                outputStyle: 'compressed'
                            }
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
                            sourceMap: false
                        }                    },
                    {
                        loader: 'postcss-loader',
                        options: {
                            postcssOptions: {
                                plugins: [
                                    ['autoprefixer', {
                                        overrideBrowserslist: ['> 1%', 'last 2 versions', 'not dead', 'not ie 11'],
                                        grid: 'autoplace',
                                        flexbox: 'no-2009'
                                    }]
                                ]
                            },
                            sourceMap: false
                        }
                    }
                ]
            },
            {
                test: /\.(woff|woff2|eot|ttf|otf)$/,
                type: 'asset/resource',
                generator: {
                    filename: 'fonts/[name].[contenthash:8][ext]'
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
                    filename: 'images/[name].[contenthash:8][ext]'
                }
            }
        ]
    },
    plugins: [
        new CleanWebpackPlugin({
            cleanOnceBeforeBuildPatterns: ['**/*', '!.gitkeep'],
            cleanStaleWebpackAssets: false,
            verbose: false
        }),
        new MiniCssExtractPlugin({
            filename: 'css/[name].[contenthash:8].css',
            chunkFilename: 'css/[name].[contenthash:8].chunk.css'
        }),        new CopyWebpackPlugin({
            patterns: [
                {
                    from: 'src/favicon.ico',
                    to: 'favicon.ico'
                }
            ]        }),        new WebpackManifestPlugin({
            fileName: 'assets-manifest.json',
            publicPath: '',
            filter: (file) => {
                // Only include JS and CSS files
                return file.name.endsWith('.js') || file.name.endsWith('.css');
            },
            map: (file) => {
                // Map actual filenames to logical names for easier lookup
                const name = file.name;
                let logicalName = name;
                
                // Map actual chunk names to logical asset paths
                if (name.startsWith('site.') && name.endsWith('.js')) {
                    logicalName = 'site.js';
                } else if (name.startsWith('validation.') && name.endsWith('.js')) {
                    logicalName = 'validation.js';
                } else if (name.startsWith('site.') && name.endsWith('.css')) {
                    logicalName = 'site.css';
                } else if (name.startsWith('vendors.') && name.endsWith('.css')) {
                    logicalName = 'vendors.css';
                }
                
                return {
                    ...file,
                    name: logicalName
                };
            }
        }),
        // Uncomment for bundle analysis
        // new BundleAnalyzerPlugin({
        //     analyzerMode: 'static',
        //     openAnalyzer: false,
        //     reportFilename: 'bundle-report.html'
        // })
    ],
    optimization: {
        minimize: true,
        minimizer: [
            new TerserPlugin({
                terserOptions: {
                    compress: {
                        drop_console: true,
                        drop_debugger: true
                    },
                    mangle: true,
                    format: {
                        comments: false
                    }
                },
                extractComments: false
            }),
            new CssMinimizerPlugin({
                minimizerOptions: {
                    preset: [
                        'default',
                        {
                            discardComments: { removeAll: true },
                            normalizeWhitespace: true,
                            colormin: true,
                            convertValues: true,
                            discardDuplicates: true,
                            discardEmpty: true,
                            mergeRules: true,
                            minifySelectors: true
                        }
                    ]
                }
            })
        ],
        splitChunks: {
            chunks: 'all',
            cacheGroups: {
                vendor: {
                    test: /[\\/]node_modules[\\/]/,
                    name: 'vendors',
                    chunks: 'all',
                    priority: 10,
                    reuseExistingChunk: true
                },
                bootstrap: {
                    test: /[\\/]node_modules[\\/]bootstrap[\\/]/,
                    name: 'bootstrap',
                    chunks: 'all',
                    priority: 20
                },
                jquery: {
                    test: /[\\/]node_modules[\\/]jquery[\\/]/,
                    name: 'jquery',
                    chunks: 'all',
                    priority: 15
                },
                common: {
                    name: 'common',
                    minChunks: 2,
                    chunks: 'all',
                    priority: 5,
                    reuseExistingChunk: true,
                    enforce: true
                }
            }
        },
        runtimeChunk: {
            name: 'runtime'
        }
    },
    resolve: {
        extensions: ['.js', '.scss', '.css'],
        alias: {
            '@': path.resolve(__dirname, 'src'),
            '@scss': path.resolve(__dirname, 'src/scss'),
            '@js': path.resolve(__dirname, 'src/js')
        }
    },
    performance: {
        maxAssetSize: 250000,
        maxEntrypointSize: 250000,
        hints: 'warning'
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
