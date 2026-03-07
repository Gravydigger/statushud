{
  description = "A Nix-flake-based C# development environment";

  inputs.nixpkgs.url = "github:NixOS/nixpkgs/nixpkgs-unstable";

  outputs = {
    self,
    nixpkgs,
  }: let
    supportedSystems = [
      "x86_64-linux"
      "aarch64-linux"
      "x86_64-darwin"
      "aarch64-darwin"
    ];

    forEachSupportedSystem = f:
      nixpkgs.lib.genAttrs supportedSystems (
        system:
          f {
            pkgs = import nixpkgs {inherit system;};
          }
      );
  in {
    devShells = forEachSupportedSystem (
      {pkgs}: {
        default = pkgs.mkShell {
          name = "csharp-dev";

          packages = with pkgs; [
            dotnet-sdk_10
            omnisharp-roslyn
            mono
            msbuild
          ];

          # Suppress .NET's telemetry
          DOTNET_CLI_TELEMETRY_OPTOUT = "1";

          shellHook = ''
            echo "C# dev shell — .NET $(dotnet --version)"
          '';
        };
      }
    );
  };
}
