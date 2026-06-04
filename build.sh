#!/usr/bin/env bash
# ============================================================
# QZXFrp 跨平台构建 (Linux / macOS)
# 用法:
#   ./build.sh linux-x64    # → dist/qzxfrp_<ver>_amd64.deb + .rpm
#   ./build.sh osx-arm64    # → dist/QZXFrp_<ver>_arm64.dmg
#   ./build.sh osx-x64      # → dist/QZXFrp_<ver>_x64.dmg
# ============================================================
set -euo pipefail

RID="${1:-}"
if [ -z "$RID" ]; then
  case "$(uname -s)" in
    Linux)  RID="linux-x64" ;;
    Darwin) RID="osx-arm64" ;;
    *)      echo "用法: $0 [linux-x64|osx-arm64|osx-x64]" ; exit 1 ;;
  esac
fi

VERSION="$(date +%Y.%m.%d)"
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PROJECT_DIR="${SCRIPT_DIR}/AvaloniaApplication1"
PUBLISH_DIR="${SCRIPT_DIR}/publish/${RID}"
DIST_DIR="${SCRIPT_DIR}/dist"

echo "=== QZXFrp 构建 ==="
echo "  版本: ${VERSION}"
echo "  运行时: ${RID}"

# ---- Publish ----
echo ""
echo "[1/2] dotnet publish ..."
dotnet publish "${PROJECT_DIR}/AvaloniaApplication1.csproj" \
  -c Release -r "${RID}" --self-contained true \
  -o "${PUBLISH_DIR}" \
  -p:Version="${VERSION}"

# ---- 平台打包 ----
echo ""
echo "[2/2] 打包 ..."
mkdir -p "${DIST_DIR}"

case "${RID}" in
  # ============ Linux ============
  linux-x64|linux-arm64)
    APP_NAME="qzxfrp"
    # RID → Debian 架构映射
    case "${RID}" in
      linux-x64)   DEB_ARCH="amd64";  RPM_ARCH="x86_64" ;;
      linux-arm64) DEB_ARCH="arm64";  RPM_ARCH="aarch64" ;;
    esac
    PKG_NAME="${APP_NAME}_${VERSION}_${DEB_ARCH}"
    BUILD_DIR="build/${PKG_NAME}"

    # --- .deb ---
    echo "  构建 .deb ..."
    rm -rf "${BUILD_DIR}"
    mkdir -p "${BUILD_DIR}/DEBIAN"
    mkdir -p "${BUILD_DIR}/usr/bin"
    mkdir -p "${BUILD_DIR}/usr/share/${APP_NAME}"
    mkdir -p "${BUILD_DIR}/usr/share/applications"

    cp -a "${PUBLISH_DIR}"/* "${BUILD_DIR}/usr/share/${APP_NAME}/"

    # 启动脚本
    cat > "${BUILD_DIR}/usr/bin/${APP_NAME}" << 'EOF'
#!/bin/bash
exec /usr/share/qzxfrp/AvaloniaApplication1 "$@"
EOF
    chmod 755 "${BUILD_DIR}/usr/bin/${APP_NAME}"

    # Desktop Entry
    cat > "${BUILD_DIR}/usr/share/applications/${APP_NAME}.desktop" << EOF
[Desktop Entry]
Name=QZXFrp
Comment=FRP 内网穿透隧道管理客户端
Exec=${APP_NAME}
Terminal=false
Type=Application
Categories=Network;
EOF

    # control
    cat > "${BUILD_DIR}/DEBIAN/control" << EOF
Package: ${APP_NAME}
Version: ${VERSION}
Section: net
Priority: optional
Architecture: ${DEB_ARCH}
Depends: libc6 (>= 2.31)
Maintainer: ZX Q <dev@qzx.fyi>
Description: FRP 内网穿透隧道管理客户端
 基于 Avalonia 的跨平台桌面客户端，管理 FRP 内网穿透隧道。
EOF

    dpkg-deb --build "${BUILD_DIR}" "${DIST_DIR}/${PKG_NAME}.deb"
    rm -rf build/
    echo "  → ${DIST_DIR}/${PKG_NAME}.deb"

    # --- .rpm ---
    if command -v rpmbuild &> /dev/null; then
      echo "  构建 .rpm ..."
      RPM_ROOT="build/rpmbuild"
      mkdir -p "${RPM_ROOT}"/{BUILD,RPMS,SOURCES,SPECS,SRPMS}
      mkdir -p "${APP_NAME}-${VERSION}"
      cp -a "${PUBLISH_DIR}"/* "${APP_NAME}-${VERSION}/"
      tar czf "${RPM_ROOT}/SOURCES/${APP_NAME}-${VERSION}.tar.gz" "${APP_NAME}-${VERSION}"
      rm -rf "${APP_NAME}-${VERSION}"

      cat > "${RPM_ROOT}/SPECS/${APP_NAME}.spec" << SPECEOF
Name:           ${APP_NAME}
Version:        ${VERSION}
Release:        1%{?dist}
Summary:        FRP 内网穿透隧道管理客户端
License:        MIT

%description
基于 Avalonia 的跨平台桌面客户端，管理 FRP 内网穿透隧道。

%install
mkdir -p %{buildroot}/usr/share/%{name}
mkdir -p %{buildroot}/usr/bin
mkdir -p %{buildroot}/usr/share/applications
cp -a * %{buildroot}/usr/share/%{name}/
cat > %{buildroot}/usr/bin/%{name} << 'EOF'
#!/bin/bash
exec /usr/share/qzxfrp/AvaloniaApplication1 "\$@"
EOF
chmod 755 %{buildroot}/usr/bin/%{name}
cat > %{buildroot}/usr/share/applications/%{name}.desktop << EOF
[Desktop Entry]
Name=QZXFrp
Exec=${APP_NAME}
Terminal=false
Type=Application
Categories=Network;
EOF

%files
/usr/share/%{name}
/usr/bin/%{name}
/usr/share/applications/%{name}.desktop
SPECEOF

      rpmbuild -bb --define "_topdir ${RPM_ROOT}" "${RPM_ROOT}/SPECS/${APP_NAME}.spec"
      cp "${RPM_ROOT}/RPMS/${RPM_ARCH}/${APP_NAME}-${VERSION}"*.rpm "${DIST_DIR}/"
      rm -rf build/
      echo "  → ${DIST_DIR}/${APP_NAME}-${VERSION}-1.${RPM_ARCH}.rpm"
    else
      echo "  ⚠ 跳过 .rpm: 未安装 rpmbuild"
    fi
    ;;

  # ============ macOS ============
  osx-x64|osx-arm64)
    ARCH="${RID#osx-}"
    APP_NAME="QZXFrp"
    DMG_NAME="${APP_NAME}_${VERSION}_${ARCH}"
    APP_BUNDLE="build/${APP_NAME}.app"

    echo "  构建 .app bundle + .dmg ..."
    rm -rf "build/"
    mkdir -p "${APP_BUNDLE}/Contents/MacOS"
    mkdir -p "${APP_BUNDLE}/Contents/Resources"

    cp -a "${PUBLISH_DIR}"/* "${APP_BUNDLE}/Contents/MacOS/"

    cat > "${APP_BUNDLE}/Contents/Info.plist" << PLIST
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0"><dict>
  <key>CFBundleExecutable</key><string>AvaloniaApplication1</string>
  <key>CFBundleIdentifier</key><string>com.qzx.qzxfrp</string>
  <key>CFBundleName</key><string>${APP_NAME}</string>
  <key>CFBundleVersion</key><string>${VERSION}</string>
  <key>CFBundleShortVersionString</key><string>${VERSION}</string>
  <key>CFBundlePackageType</key><string>APPL</string>
  <key>LSMinimumSystemVersion</key><string>10.15</string>
  <key>NSHighResolutionCapable</key><true/>
</dict></plist>
PLIST

    echo "APPL????" > "${APP_BUNDLE}/Contents/PkgInfo"

    # DMG
    DMG_TMP="build/dmg_tmp"
    mkdir -p "${DMG_TMP}"
    cp -R "${APP_BUNDLE}" "${DMG_TMP}/"
    ln -s /Applications "${DMG_TMP}/Applications"

    DMG_PATH="${DIST_DIR}/${DMG_NAME}.dmg"
    hdiutil create -volname "${APP_NAME}" \
      -srcfolder "${DMG_TMP}" -ov -format UDZO "${DMG_PATH}"

    rm -rf build/
    echo "  → ${DMG_PATH}"
    echo "  注意: DMG 未签名。分发前需 codesign + notarization。"
    ;;

  *)
    echo "  未知 RID: ${RID}"
    exit 1
    ;;
esac

echo ""
echo "=== 构建完成 ==="
ls -lh "${DIST_DIR}/"
