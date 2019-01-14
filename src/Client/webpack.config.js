var path = require("path");
var webpack = require("webpack");
var MinifyPlugin = require("terser-webpack-plugin");

function resolve(filePath) {
    return path.join(__dirname, filePath)
}

// passing -p on the command line to webpack specifies production mode, otherwise debug is used.
var isProduction = process.argv.indexOf("-p") >= 0;

console.log("Bundling for " + (isProduction ? "production" : "development") + "...");

// Use babel-preset-env to generate JS compatible with most-used browsers.
// More info at https://github.com/babel/babel/blob/master/packages/babel-preset-env/README.md
var babelOptions = {
    presets: [
        ["@babel/preset-env", {
            "targets": {
                "browsers": ["last 2 versions"]
            },
            "modules": false,
            "useBuiltIns": "usage",
        }]
    ],
    // A plugin that enables the re-use of Babel's injected helper code to save on codesize.
    plugins: ["@babel/plugin-transform-runtime"]
};

module.exports = {
    entry: {
        "app": [
            "whatwg-fetch",
            "@babel/polyfill",
            resolve("./Client.fsproj")
        ]
    },
    output: {
        path: resolve('./public/js'),
        publicPath: "/js",
        filename: "[name].js"
    },
    // Set up default webpack optimisations for prod or dev builds
    mode: isProduction ? "production" : "development",
    // Turn on source maps when debugging
    devtool: isProduction ? undefined : "source-map",
    // Turn off symlinks for module resolution
    resolve: { symlinks: false },
    optimization: {
        // Split the code coming from npm packages into a different file.
        // 3rd party dependencies change less often, let the browser cache them.
        splitChunks: {
            cacheGroups: {
                commons: {
                    test: /node_modules/,
                    name: "vendors",
                    chunks: "all"
                }
            }
        },
        // In production, turn on minification to make JS files smaller
        minimizer: isProduction ? [new MinifyPlugin({
            terserOptions: {
              compress: {
                  // See https://github.com/SAFE-Stack/SAFE-template/issues/190
                  inline: false
              }
            }
          })] : []
    },
    // In development, enable hot reloading when code changes without refreshing the browser or losing state.
    plugins: isProduction ? [] : [
        new webpack.HotModuleReplacementPlugin(),
        new webpack.NamedModulesPlugin()
    ],
    // Configuration for the development server
    devServer: {
        // redirect requests that start with /api/* to the server on port 8085
        proxy: {
            '/api/*': {
                target: 'http://localhost:' + (process.env.SERVER_PROXY_PORT || "8085"),
                changeOrigin: true
            }
        },
        // turn on hot module reloading
        hot: true,
        // more automatic reloading
        inline: true,
        // default page
        historyApiFallback: { index: resolve("./index.html") },
        // where to server static files from
        contentBase: resolve("./public")
    },
    // The modules that are used by webpack for transformations
    module: {
        rules: [
            {
                // - fable-loader: transforms F# into JS
                test: /\.fs(x|proj)?$/,
                use: {
                    loader: "fable-loader",
                    options: {
                        babel: babelOptions,
                        define: isProduction ? [] : ["DEBUG"]
                   }
                },
            },
            {
                // - babel-loader: transforms JS to old syntax (compatible with old browsers)
                test: /\.js$/,
                exclude: /node_modules/,
                use: {
                    loader: 'babel-loader',
                    options: babelOptions
                },
            }
        ]
    }
};
