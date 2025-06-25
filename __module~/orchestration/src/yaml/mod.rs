use std::fs;

use anyhow::{Context, Result};
use arpx::Runtime;
use arpx::runtime::profile::deserialize::Profile;
use log::debug;

// impl Profile {
    pub fn load_runtime(path: &str, job_names: &[String]) -> Result<Runtime> {
        debug!("Loading profile from path: {}", path);

        let data = fs::read_to_string(path).context("Error reading file")?;
        let profile = deserialize_from_str(&data).context("Error deserializing file")?;

        RuntimeBuilder::from_profile_and_job_names(profile, job_names)
            .context("Error building runtime")
    }

    fn deserialize_from_str(data: &str) -> Result<Profile> {
        debug!("Deserializing profile data");

        serde_yaml::from_str(data).map_err(Error::new)
    }
// }
