const js = require('@eslint/js');

module.exports = [
    js.configs.recommended,
    {
        files: ["src/**/*.js"],
        languageOptions: {
            ecmaVersion: 2022,
            sourceType: "module",
            globals: {
                // Browser globals
                window: "readonly",
                document: "readonly",
                console: "readonly",
                setTimeout: "readonly",
                clearTimeout: "readonly",
                setInterval: "readonly",
                clearInterval: "readonly",
                
                // Libraries
                bootstrap: "readonly",
                jQuery: "readonly",
                $: "readonly",
                
                // Node/webpack globals
                require: "readonly",
                module: "readonly",
                exports: "readonly",
                __dirname: "readonly",
                __filename: "readonly",
                process: "readonly"
            }
        },
        rules: {
            "no-unused-vars": ["warn", { "argsIgnorePattern": "^_" }],
            "no-console": "off", // Allow console for now, can enable later
            "prefer-const": "error",
            "no-var": "error",
            "eqeqeq": "error",
            "curly": "error",
            "no-multi-spaces": "error",
            "no-multiple-empty-lines": ["error", { "max": 2 }],
            "indent": ["error", 4],
            "quotes": ["error", "single"],
            "semi": ["error", "always"]
        }
    }
];
