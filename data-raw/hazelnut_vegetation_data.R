## code to prepare `SWELLparameters` dataset goes here
# Read the CSV files
hazelnut_evi <- read.csv('data-raw/hazelnut_evi.csv')
hazelnut_ndvi <- read.csv('data-raw/hazelnut_ndvi.csv')

# Perform a left join (merge in base R)
hazelnut_vegetation_data <- merge(
  x = hazelnut_evi,
  y = hazelnut_ndvi,
  all.x = TRUE
)

# Filter rows where year >= 2009
hazelnut_vegetation_data <- hazelnut_vegetation_data[hazelnut_vegetation_data$year >= 2009, ]

# Select specific columns
hazelnut_vegetation_data <- hazelnut_vegetation_data[
  , c("orchard_id", "Municipality", "latitude", "longitude", "year", "doy", "ndvi", "evi")
]

#usethis::use_data_raw("hazelnut_vegetation_data")
# Save the dataset to the `data/` folder of the package
usethis::use_data(hazelnut_vegetation_data, overwrite = TRUE)

