#!/usr/bin/env bash
# AAB에서 universal APK를 뽑아 업로드 키로 서명한다.
# 사용: bash Tools/export-universal-apk.sh <input.aab> <output.apk>
# 환경 변수: KEYSTORE_BASE64, KEYSTORE_PASS, KEYALIAS_NAME, KEYALIAS_PASS
set -euo pipefail

BUNDLETOOL_VERSION="1.18.3"

INPUT_AAB="$1"
OUTPUT_APK="$2"

# 키스토어가 작업 폴더에 남으면 안 되므로 임시 폴더에 두고 끝나면 삭제
WORK_DIR="$(mktemp -d)"
trap 'rm -rf "$WORK_DIR"' EXIT

: "${KEYSTORE_BASE64:?}" "${KEYSTORE_PASS:?}" "${KEYALIAS_NAME:?}" "${KEYALIAS_PASS:?}"

curl -sSL -o "$WORK_DIR/bundletool.jar" \
  "https://github.com/google/bundletool/releases/download/$BUNDLETOOL_VERSION/bundletool-all-$BUNDLETOOL_VERSION.jar"
echo "$KEYSTORE_BASE64" | base64 -d > "$WORK_DIR/upload.keystore"

java -jar "$WORK_DIR/bundletool.jar" build-apks \
  --bundle="$INPUT_AAB" --output="$WORK_DIR/out.apks" --mode=universal \
  --ks="$WORK_DIR/upload.keystore" --ks-pass="pass:$KEYSTORE_PASS" \
  --ks-key-alias="$KEYALIAS_NAME" --key-pass="pass:$KEYALIAS_PASS"

unzip -p "$WORK_DIR/out.apks" universal.apk > "$OUTPUT_APK"
echo "Exported: $OUTPUT_APK"
