module.exports = {
    plugins: [
        require('autoprefixer'),
        require('postcss-preset-env')({
            stage: 1,
            features: {
                'nesting-rules': true,
                'custom-properties': true
            }
        })
    ]
};
