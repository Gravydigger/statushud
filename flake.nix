{
  description = "Vintage Story development enviroment";

  inputs = {
    nixpkgs.url = "github:nixos/nixpkgs?ref=nixos-unstable";
  };

  outputs = {
    self,
    nixpkgs,
  }: let
    system = "x86_64-linux";
    pkgs = import nixpkgs {
      inherit system;
      config.allowUnfree = true;
    };
  in {
    devShells.x86_64-linux.default = pkgs.mkShell {
      packages = with pkgs; [
        (with dotnetCorePackages;
          combinePackages [
            sdk_8_0
          ])
        # Used for running the ZZCakeBuild binary
        steam-run
      ];
      # shellHook = ''
      #   echo "Welcome to the devShell!"
      # '';
      #VINTAGE_STORY = "${pkgs.vintagestory}/share/vintagestory";
    };
  };
}
