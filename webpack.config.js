const path = require("path");

module.exports = {
    entry: path.join(__dirname, "src", "App.fsx"),
    output: {
        path: path.join(__dirname, "public"),
        filename: "bundle.js",
        chunkFilename: "[name].bundle.js",
        publicPath: "/"
    },
    optimization: {
        splitChunks: {
            chunks: "async"
        }
    },
    devServer: {
        contentBase: path.join(__dirname, "public"),
        historyApiFallback: true
    },
    module: {
        rules: [
            {
                test: /\.fs(x|proj)?$/,
                use: "fable-loader"
            }
        ]
    }
};