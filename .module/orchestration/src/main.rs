use arpx::Runtime;
use std::fs::{create_dir_all, File};
use std::io::Write;
use std::path::Path;

fn main() -> Result<(), Box<dyn std::error::Error>> {
    println!("Starting ArduPilot binary download example using arpx...");

    let profile_content = r#"
jobs:
  download_firmware_job: |
    [ download_plane_binary; ]

processes:
  download_plane_binary:
    command: "curl -fL -o ArduPlane_Pixhawk1_stable.apj https://firmware.ardupilot.org/Plane/stable/Pixhawk1/arduplane.apj"
    cwd: "./firmware_downloads"
    onsucceed: arpx_exit_success
    onfail: arpx_exit_error
"#;

    let profile_path = Path::new("arpx_download_profile.yaml");
    let download_dir = Path::new("./firmware_downloads");

    // Create download directory
    if !download_dir.exists() {
        create_dir_all(download_dir)?;
        println!("Created download directory: {:?}", download_dir.canonicalize().unwrap_or_else(|_| download_dir.to_path_buf()));
    } else {
        println!("Download directory already exists: {:?}", download_dir.canonicalize().unwrap_or_else(|_| download_dir.to_path_buf()));
    }

    // Write YAML profile to a file
    let mut file = File::create(profile_path)?;
    file.write_all(profile_content.as_bytes())?;
    println!("Created arpx profile: {:?}", profile_path.canonicalize().unwrap_or_else(|_| profile_path.to_path_buf()));

    // Create and run the arpx runtime
    match Runtime::from_profile(profile_path, None) {
        Ok(mut runtime) => {
            println!("ARPX Runtime created from profile. Starting job 'download_firmware_job'...");
            let success = runtime.run("download_firmware_job");
            if success {
                println!("ARPX job 'download_firmware_job' completed successfully.");
                let downloaded_file_path = download_dir.join("ArduPlane_Pixhawk1_stable.apj");
                println!("Check for the downloaded file: {:?}", downloaded_file_path);
            } else {
                eprintln!("ARPX job 'download_firmware_job' failed.");
            }
        }
        Err(e) => {
            eprintln!("Failed to create ARPX Runtime from profile: {}", e);
            // Attempt to clean up profile file even on error, ignoring cleanup error
            if profile_path.exists() {
                let _ = std::fs::remove_file(profile_path);
            }
            return Err(Box::new(e));
        }
    }

    // Clean up the profile file
    if profile_path.exists() {
        std::fs::remove_file(profile_path)?;
        println!("Cleaned up arpx profile: {}", profile_path.to_string_lossy());
    }

    Ok(())
}
