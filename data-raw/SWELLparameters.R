## code to prepare `SWELLparameters` dataset goes here
SWELLparameters<-read.csv(paste0(getwd(),'/data-raw/photothermalRequirements.csv'))

# Convert 'calibration' column: "x" → TRUE, empty → FALSE
SWELLparameters$calibration <- SWELLparameters$calibration == "x"

# Transform into a nested list: Species → Class → Parameter
SWELLparameters <- split(SWELLparameters, SWELLparameters$species)  # Group by species
SWELLparameters <- lapply(SWELLparameters, function(species_df) {
  class_list <- split(species_df, species_df$class)  # Group by class within each species

  lapply(class_list, function(class_df) {
    param_list <- split(class_df, class_df$parameter)  # Group by parameter

    lapply(param_list, function(param_df) {
      # Keep only relevant columns
      param_values <- param_df[, c("min", "max", "value", "calibration"), drop = FALSE]

      # Convert to list format
      as.list(param_values)
    })
  })
})

# Save the dataset to a file
usethis::use_data(SWELLparameters,overwrite = T)
