## code to prepare `SWELLparameters` dataset goes here
SWELLparameters<-read.csv(paste0(getwd(),'/data-raw/photothermalRequirements.csv'))

# Transform into a nested list
SWELLparameters <- split(SWELLparameters, SWELLparameters$class)
SWELLparameters <- lapply(SWELLparameters, function(class_df) {
  split(class_df, class_df$parameter)
})
SWELLparameters <- lapply(SWELLparameters, function(class_list) {
  lapply(class_list, function(param_df) {
    param_df <- param_df[ , c("min", "max", "value", "calibration")]
    as.list(param_df)
  })
})


# Save the dataset to a file
usethis::use_data(SWELLparameters,overwrite = T)
