let
  nixpkgs = fetchTarball "https://github.com/NixOS/nixpkgs/tarball/nixos-24.05";

  pkgs = import nixpkgs {
    config = {};
    overlays = [];
  };
in {
  vs = pkgs.callPackage ./vintage_story.nix {};
}
