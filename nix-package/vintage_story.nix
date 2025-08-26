{
  lib,
  stdenv,
  fetchurl,
  makeWrapper,
  makeDesktopItem,
  copyDesktopItems,
  xorg,
  gtk2,
  sqlite,
  openal,
  cairo,
  libGLU,
  SDL2,
  freealut,
  libglvnd,
  pipewire,
  libpulseaudio,
  dotnet-runtime_8,
}:

stdenv.mkDerivation rec {
  pname = "vintagestory";
  version = "1.21.0";
  release = "stable";

  src = fetchurl {
    url = "https://cdn.vintagestory.at/gamefiles/${release}/vs_client_linux-x64_${version}.tar.gz";
    hash = "sha256-90YQOur7UhXxDBkGLSMnXQK7iQ6+Z8Mqx9PEG6FEXBs=";
  };

  nativeBuildInputs = [
    makeWrapper
    copyDesktopItems
  ];

  runtimeLibs = lib.makeLibraryPath (
    [
      gtk2
      sqlite
      openal
      cairo
      libGLU
      SDL2
      freealut
      libglvnd
      pipewire
      libpulseaudio
    ]
    ++ (with xorg; [
      libX11
      libXi
      libXcursor
    ])
  );

  installPhase = ''
    runHook preInstall

    mkdir -p $out/vintagestory
    cp -r * $out/vintagestory

    runHook postInstall
  '';

  preFixup = ''
    makeWrapper ${dotnet-runtime_8}/bin/dotnet $out/vintagestory/Vintagestory \
      --prefix LD_LIBRARY_PATH : "${runtimeLibs}" \
      --prefix FONTCONFIG_FILE : "$out/font.conf" \
      --set-default mesa_glthread true \
      --add-flags $out/vintagestory/Vintagestory.dll

    makeWrapper ${dotnet-runtime_8}/bin/dotnet $out/vintagestory/VintagestoryServer \
      --prefix LD_LIBRARY_PATH : "${runtimeLibs}" \
      --set-default mesa_glthread true \
      --add-flags $out/vintagestory/VintagestoryServer.dll

    find "$out/vintagestory/assets/" -not -path "*/fonts/*" -regex ".*/.*[A-Z].*" | while read -r file; do
      local filename="$(basename -- "$file")"
      ln -sf "$filename" "''${file%/*}"/"''${filename,,}"
    done
  '';

  meta = {
    description = "In-development indie sandbox game about innovation and exploration";
    homepage = "https://www.vintagestory.at/";
    license = lib.licenses.unfree;
    sourceProvenance = [lib.sourceTypes.binaryBytecode];
    platforms = lib.platforms.linux;
    maintainers = with lib.maintainers; [
      gravydigger
    ];
    mainProgram = "vintagestory";
  };
}
