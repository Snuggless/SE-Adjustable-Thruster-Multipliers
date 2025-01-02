var fs = require('fs');
const { exit } = require('process');

const config = {
    name: "AdjustableThrusterMultipliers",
    outputDir: process.env.APPDATA + "/SpaceEngineers/Mods/",
    inputDir: "./AdjustableThrusterMultipliers"
}

if (fs.existsSync(config.inputDir) == false) {
    console.log("Input directory is invalid");
    process.exit(1);
}

function createFolder(path, recursive) {
    if (fs.existsSync(path) == false) {
        fs.mkdirSync(path, { recursive: recursive });
    }
}

function copyFiles(src, dest, ext) {
    if (ext) {
        fs.readdirSync(src).forEach((file) => {
            if (file.endsWith(ext)) {
                fs.cpSync(src + "/" + file, dest + "/" + file, { recursive: true, force: true });
            }
        });
        return;
    }

    fs.cpSync(src, dest, { recursive: true, force: true });
}

createFolder(config.outputDir + config.name);
createFolder(config.outputDir + config.name + "/Data");
createFolder(config.outputDir + config.name + "/Data/Scripts");
createFolder(config.outputDir + config.name + "/Data/Scripts/AdjustableThrusterMultipliers");

copyFiles(config.inputDir + "/Assets", config.outputDir + config.name + "/");
copyFiles(config.inputDir + "/Properties", config.outputDir + config.name + "/Data/", ".sbc");
copyFiles(config.inputDir + "/", config.outputDir + config.name + "/Data/Scripts/AdjustableThrusterMultipliers/", ".cs");

console.log("Build Success");