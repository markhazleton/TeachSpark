const devConfig = require('./webpack.config.dev');
const prodConfig = require('./webpack.config.prod');

module.exports = (env, argv) => {
    const isProduction = argv.mode === 'production';
    return isProduction ? prodConfig : devConfig;
};
