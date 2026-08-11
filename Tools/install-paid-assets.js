const fs = require('fs');
const path = require('path');
const { execSync } = require('child_process');

const deps = JSON.parse(fs.readFileSync('asset-dependencies.json', 'utf8'));
const paidAssetsRoot = process.env.HOME + '/paid-assets';

for (const asset of deps.assets) {
    const src = path.join(paidAssetsRoot, asset.installPath);
    const dest = path.join('.', asset.installPath);

    fs.mkdirSync(path.dirname(dest), { recursive: true });
    execSync(`cp -r "${src}" "${dest}"`);
    console.log(`Installed: ${asset.name} → ${asset.installPath}`);
}