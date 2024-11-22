# .NET Aspire - local server container test

.NET 9, Aspire 9.0.

## Local testing

1. Install [Podman Desktop](https://podman.io/). Install all components, including turning on the virtualization platform, and use WSLv2 unless you have a reason not to. Create a Podman VM with the default image settings.
   - When not using Podman Desktop and container hosting you can list running WSL VMs with `wsl --list --running` and shut down the Podman VM with: `wsl --terminate podman-machine-default`
