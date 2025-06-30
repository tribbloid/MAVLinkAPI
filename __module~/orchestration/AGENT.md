# AGENT.md - MAVLink Orchestration Project

## Build/Test Commands
- **Build**: `cargo build` (compile the project)
- **Check**: `cargo check` (analyze without building) 
- **Test**: `cargo test` (run all tests)
- **Run main**: `cargo run` (execute main.rs)
- **Run examples**: `cargo run --example downloadSITL` or `cargo run --example minimal`
- **Format**: `cargo fmt` (format code)
- **Lint**: `cargo clippy` (linting)

## Architecture
- **Main crate**: mavlink-orchestration using arpx runtime for job orchestration
- **Key dependency**: arpx v0.5.0 for YAML-defined job execution
- **Configuration**: YAML files define jobs/processes (minimal.yml, download_sitl.yml)
- **Examples**: downloadSITL.rs (downloads ArduPilot SITL), minimal.rs (basic demo)

## Code Style
- **Edition**: Rust 2024
- **Error handling**: Use `Result<(), Box<dyn std::error::Error>>` pattern
- **Imports**: Group std imports, then external crates (arpx), then internal
- **Naming**: snake_case for variables/functions, PascalCase for types
- **Logging**: Use println! with emoji prefixes (🚀, ✅, ❌, ⚠️, 📋, etc.)
- **Path handling**: Use `std::path::Path` for file operations
- **String formatting**: Prefer format strings and `.display()` for paths
