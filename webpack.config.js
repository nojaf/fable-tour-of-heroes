const path = require("path");

module.exports = {
    entry: path.join(__dirname,"src","App.fsx"),
    output:{
        path: path.join(__dirname, "public"),
        filename:"bundle.js"
    },
    devServer:{
        contentBase:"./public",
        historyApiFallback: true
    },
    module:{
        rules:[
            {
                test: /.fs(x|proj)?$/,
                use:"fable-loader"
            }
        ]
    }
};