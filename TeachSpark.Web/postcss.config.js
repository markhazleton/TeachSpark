module.exports = {
    plugins: [
        require('autoprefixer')({
            overrideBrowserslist: ['> 1%', 'last 2 versions', 'not dead', 'not ie 11'],
            grid: 'autoplace',
            flexbox: 'no-2009'
        }),
        require('postcss-preset-env')({
            stage: 2,
            features: {
                'nesting-rules': true,
                'custom-properties': true,
                'logical-properties-and-values': false,
                'prefers-color-scheme-query': false,
                'gap-properties': false,
                'custom-media-queries': false,
                'media-query-ranges': false,
                'is-pseudo-class': false,
                'focus-within-pseudo-class': false,
                'focus-visible-pseudo-class': false,
                'color-functional-notation': false,
                'hexadecimal-alpha-notation': false,
                'lab-function': false,
                'oklab-function': false,
                'color-function': false,
                'hwb-function': false,
                'case-insensitive-attributes': false
            },
            autoprefixer: false, // We're using autoprefixer separately
            preserve: false
        })
    ]
};
