use arpx::Runtime;
use std::path::Path;

fn main() -> Result<(), Box<dyn std::error::Error>> {
    println!("🚀 Starting arpx runtime to execute minimal.yml...");
    
    // Load the YAML profile
    let profile_path = Path::new("minimal.yml");
    
    if !profile_path.exists() {
        eprintln!("❌ Error: minimal.yml not found in current directory");
        return Err("YAML file not found".into());
    }
    
    println!("📋 Loading profile from: {}", profile_path.display());
    
    // Create runtime from the YAML profile file
    // The second argument specifies which jobs to run
    let job_names = vec!["foo".to_string()];
    let runtime = Runtime::from_profile(
        profile_path.to_str().ok_or("Invalid path")?, 
        &job_names
    )?;
    
    println!("✅ YAML profile loaded successfully!");
    println!("⚡ Starting execution...");
    
    // Execute the runtime
    runtime.run()?;
    
    println!("🎉 Runtime execution completed successfully!");
    println!("📄 The 'arduplane' SITL binary should be downloaded from the YAML job definition.");
    
    // Verify the download
    match std::fs::metadata("arduplane") {
        Ok(metadata) => {
            println!("📊 Downloaded file size: {} bytes", metadata.len());
        }
        Err(_) => {
            println!("⚠️  Warning: Could not verify downloaded file");
        }
    }
    
    Ok(())
}
