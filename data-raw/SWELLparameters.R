## code to prepare `SWELLparameters` dataset goes here
SWELLparameters<-read.csv('C:\\Users\\simoneugomaria.brega\\Dropbox\\IO\\SWELL\\inst\\extdata\\files\\parametersData\\photothermalRequirements.csv')

SWELLparameters$calibration<-ifelse(SWELLparameters$calibration == "", F, T)
#usethis::use_data_raw("SWELLparameters")

# Save the dataset to a file
usethis::use_data(SWELLparameters,overwrite = T)
