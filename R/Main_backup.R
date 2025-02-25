library(jsonlite)

#' Run SWELL Calibration
#'
#' This function performs the calibration of the SWELL model using weather data as input and vegetation indices data (NDVI or EVI) as reference data using a multi-start simplex algorithm.
#'
#' @param weather_data A data frame containing geographical and weather data.
#'   Columns must be: Latitude (numeric), Longitude (numeric), Date (YYYY-MM-DD), Tmin (numeric, °C), Tmax (numeric, °C).
#' @param vegetation_data A data frame containing the vegetation index data.
#'   Columns must be: PixelID (string), Group (string), Year (integer), Doy (integer), Longitude (numeric), Latitude (numeric), VegetationIndex (numeric).
#' @param vegetation_index The vegetation index used for SWELL calibration (string). Available options are 'EVI' and 'NDVI'.
#' @param SWELLparameters A nested list structured as `SWELLparameters$species$class$parameter`,
#'   containing SWELL model parameters. Each parameter is itself a list with the following elements:
#'   - `min`: Minimum value of the parameter (numeric), used for calibration.
#'   - `max`: Maximum value of the parameter (numeric), used for calibration.
#'   - `value`: Default value of the parameter (numeric).
#'   - `calibration`: Logical (`TRUE` if the parameter is under calibration, `FALSE` otherwise).
#' @param species the species to be used: it must be present in SWELLparameters as the first level  of the named list
#' @param start_year Start year for calibration (default: 2011).
#' @param end_year End year for calibration (default: 2022).
#' @param simplexes Number of simplexes for calibration (default: 3).
#' @param iterations Number of iterations for calibration (default: 1000).
#' @return A list containing:
#'   - calibration_results: A data frame combining all calibration results with SWELL outputs.
#'   - parameters_pixels: A data frame containing the calibrated values of SWELL parameters.
#'   - parameters_group: A data frame containing the mean and standard deviation of SWELL parameters grouped by the Group column in the vegetation_data dataframe.
#' @export
swellCalibration <- function(weather_data, vegetation_data,
                             vegetationIndex = "EVI",
                             SWELLparameters, species = 'beech',
                             start_year = 2011, end_year = 2022,
                             simplexes = 1, iterations = 1) {

  #### Input Validation ####
  # Check if weather_data is a data frame
  if (!is.data.frame(weather_data)) {
    stop("'weather_data' object must be a data frame.")
  }

  # Check if vegetation_data is a data frame
  if (!is.data.frame(vegetation_data)) {
    stop("'vegetation_data' must be a data frame.")
  }

  # Check if vegetationIndex is valid
  if (!species %in% names(SWELLparameters)) {
    sp <- names(SWELLparameters)
    for (i in 1:length(unique(sp))){
      print(sp[i])
    }
    stop("'species' is not valid. Please see above the available species or add a new species to SWELLparameters list.")
  }

  # Check if vegetationIndex is valid
  if (!vegetationIndex %in% c("EVI", "NDVI")) {
    stop("'vegetationIndex' must be either 'EVI' or 'NDVI'.")
  }

  # Check if SWELLparameters is a named list
  if (!is.list(SWELLparameters) || is.null(names(SWELLparameters))) {
    stop("'SWELLparameters' must be a named list.")
  }

  # Check if start_year and end_year are numeric and valid
  if (!is.numeric(start_year) || !is.numeric(end_year)) {
    stop("'start_year' and 'end_year' must be numeric.")
  }
  if (start_year > end_year) {
    stop("'start_year' must be less than or equal to 'end_year'.")
  }

  # Check if 'Date' column exists in weather_data
  if (!"Date" %in% colnames(weather_data)) {
    stop("'weather_data' must contain a column named 'Date'.")
  }

  # Check if 'Date' column is of type Date
  if (!inherits(weather_data$Date, "Date")) {
    stop("'Date' column in 'weather_data' must be of type Date.")
  }

  # Check if start_year and end_year are present in the weather_data
  weather_years <- unique(format(weather_data$Date, "%Y"))
  if (!as.character(start_year) %in% weather_years) {
    stop(paste0("Start year ", start_year, " is not present in 'weather_data'."))
  }
  if (!as.character(end_year) %in% weather_years) {
    stop(paste0("End year ", end_year, " is not present in 'weather_data'."))
  }

  # Check if simplexes and iterations are positive integers
  if (!is.numeric(simplexes) || simplexes <= 0 || simplexes != as.integer(simplexes)) {
    stop("'simplexes' must be a positive integer.")
  }
  if (!is.numeric(iterations) || iterations <= 0 || iterations != as.integer(iterations)) {
    stop("'iterations' must be a positive integer.")
  }

  #### Path Configuration ####
  exe_path <- switch(Sys.info()["sysname"],
                     "Windows" = system.file("extdata", "Windows", "runner.exe", package = "SWELL"),
                     "Darwin"  = system.file("extdata", "macOS", "runner", package = "SWELL"),
                     "Linux"   = system.file("extdata", "Linux", "runner", package = "SWELL"))

  if (!file.exists(exe_path)) {
    stop("Executable not found at the specified path: ", exe_path)
  }

  config_folder <- dirname(exe_path)
  if (!dir.exists(config_folder)) {
    stop("Config folder does not exist: ", config_folder)
  }

  if (missing(config_folder)) {
    config_folder <- tempdir()
  }

  #### Prepare Weather Data ####
  weather_data$fileName <- paste0(weather_data$Latitude, "_", weather_data$Longitude)

  if ("Date" %in% names(weather_data)) {
    weather_data$Date <- gsub("^\"|\"$", "", as.character(weather_data$Date))
  }

  weather_dir <- file.path(config_folder, "weather")
  weather_file <- file.path(weather_dir, paste0(unique(weather_data$fileName), ".csv"))
  ndvi_file <- file.path(config_folder, "ndvi_data.csv")
  parameters_file <- file.path(config_folder, "parameters.csv")

  dir.create(weather_dir, showWarnings = FALSE, recursive = TRUE)
  dir.create(file.path(config_folder, "outputsCalibration"), showWarnings = FALSE, recursive = TRUE)
  dir.create(file.path(config_folder, "outputsValidation"), showWarnings = FALSE, recursive = TRUE)
  dir.create(file.path(config_folder, "outputsParameters"), showWarnings = FALSE, recursive = TRUE)

  #### Write Weather Data ####
  unique_coords <- unique(weather_data[, c("Latitude", "Longitude")])

  for (i in seq_along(weather_file)) {
    file_name <- sub(".*/([^/]+)\\.csv$", "\\1", weather_file[i])
    filtered_data <- subset(weather_data, fileName == file_name)
    write.table(filtered_data, file = weather_file[i], sep = ",", row.names = FALSE, col.names = TRUE, quote = FALSE)
  }

  #### Write Vegetation Data ####
  write.table(vegetation_data, file = ndvi_file, sep = ",", row.names = FALSE, col.names = TRUE, quote = FALSE)

  SWELLparameters_df <- do.call(rbind, lapply(names(SWELLparameters), function(species) {
    do.call(rbind, lapply(names(SWELLparameters[[species]]), function(class) {
      do.call(rbind, lapply(names(SWELLparameters[[species]][[class]]), function(param) {
        param_list <- SWELLparameters[[species]][[class]][[param]]

        # Ensure numeric values are present and valid
        min_value <- ifelse(is.null(param_list$min), 0, param_list$min)
        max_value <- ifelse(is.null(param_list$max), 1, param_list$max)
        value <- ifelse(is.null(param_list$value), (min_value + max_value) / 2, param_list$value)
        calibration_flag <- ifelse(is.null(param_list$calibration), FALSE, param_list$calibration)

        data.frame(
          species = species,  # Include species column
          class = class,
          parameter = param,
          min = as.numeric(min_value),
          max = as.numeric(max_value),
          value = as.numeric(value),
          calibration = ifelse(calibration_flag, "x", "")  # Convert TRUE → "x", FALSE → ""
        )
      }))
    }))
  }))

  # Write the table to file
  write.table(SWELLparameters_df, file = parameters_file,
              sep = ",", row.names = FALSE, col.names = TRUE, quote = FALSE)


  # Prepare configuration file
  swell_config <- list(
    settings = list(
      calibration = "true",
      species = species,
      startYear = as.character(start_year),
      endYear = as.character(end_year),
      weatherDirectory = normalizePath(weather_dir),
      referenceDataFile = normalizePath(ndvi_file),
      parametersDataFile = normalizePath(parameters_file),
      validationReplicates = "1",
      parametersDistributions = "uniform",
      parametersValidationFile = "",
      simplexes = as.character(simplexes),
      iterations = as.character(iterations),
      vegetationIndex = vegetationIndex,
      outputCalibrationDir = normalizePath(file.path(config_folder, "outputsCalibration")),
      outputValidationDir = normalizePath(file.path(config_folder, "outputsValidation")),
      outputParametersDir = normalizePath(file.path(config_folder, "outputsParameters")),
      startPixel = "0",
      numberPixels = "5000"
    )
  )

  # Save configuration to JSON
  json_file <- file.path(config_folder, "SWELLconfig.json")
  jsonlite::write_json(swell_config, json_file, auto_unbox = TRUE)

  # Execute runner program
  cmd <- paste(shQuote(exe_path), shQuote(json_file))
  message("Running command: ", cmd)
  result <- tryCatch({
    # Run the C# executable and capture output
    system(cmd, intern = F)

    list(success = TRUE)
  }, error = function(e) {
    list(success = FALSE, error = e$message)
  })

  if (!result$success) {
    stop("Error while running executable: ", result$error)
  }

  # Load calibration results
  output_dir <- file.path(config_folder, "outputsCalibration")
  csv_files <- list.files(output_dir, pattern = "\\.csv$", full.names = TRUE)
  if (length(csv_files) == 0) {
    stop("No CSV files found in the directory: ", output_dir)
  }

  # Combine results into a single data frame
  results <- do.call(rbind, lapply(csv_files, read.csv, stringsAsFactors = FALSE))
  # Update rates in results data frame
  results$date<-as.Date(results$date, format = "%m/%d/%Y")
  results$dormancyInductionRate <- ifelse(results$dormancyPercentage == 100, 0, results$dormancyInductionRate)
  results$endodormancyRate <- ifelse(results$dormancyInductionRate == 0, results$endodormancyRate, 0)
  results$growthRate <- ifelse(results$ecodormancyRate == 0, results$growthRate, 0)
  results$greendownRate <- ifelse(results$growthRate > 0 | results$declineRate > 0, 0, results$greendownRate)
  results$dormancyInductionRate <- ifelse(results$greendownRate > 0, 0, results$dormancyInductionRate)


  # Load calibrated parameters
  output_dir_parameters <- file.path(config_folder, "outputsParameters")
  csv_files <- list.files(output_dir_parameters, pattern = "\\.csv$", full.names = TRUE)

  if (length(csv_files) == 0) {
    stop("No CSV files found in the directory: ", output_dir_parameters)
  }

  # Combine results into a single data frame
  resultsParameters <- do.call(rbind, lapply(csv_files, read.csv, stringsAsFactors = FALSE))

  # Splitting 'param' column to extract species, class, and parameter
  split_cols <- sapply(strsplit(resultsParameters$param, "_"), `[`)
  resultsParameters$species <- species
  resultsParameters$class <- split_cols[1, ]
  resultsParameters$param <- split_cols[2, ]

  # Rename and rearrange columns
  colnames(resultsParameters)[2] <- "group"  # Rename the second column to 'group'

  # Match calibrated parameters with the corresponding min/max values from SWELLparameters
  resultsParameters$min <-
    sapply(seq_along(resultsParameters$param), function(i) {
      SWELLparameters[[resultsParameters$species[i]]][[resultsParameters$class[i]]][[resultsParameters$param[i]]]$min
    })

  resultsParameters$max <-
    sapply(seq_along(resultsParameters$param), function(i) {
      SWELLparameters[[resultsParameters$species[i]]][[resultsParameters$class[i]]][[resultsParameters$param[i]]]$max
    })

  # Rearrange columns for clarity
  resultsParameters <- resultsParameters[, c("pixelID", "group", "species", "class", "param", "min", "max", "value")]

  # Define columns for grouping
  grouping_columns <- c("group", "species", "class", "param", "min", "max")

  # Dynamically create the formula for aggregation
  aggregation_formula <- as.formula(paste("value ~", paste(grouping_columns, collapse = " + ")))

  colnames(resultsParameters)
  # Perform the aggregation
  resultsParametersGroup <- aggregate(aggregation_formula,
                                      data = resultsParameters,
                                      FUN = function(x) c(mean = round(mean(x), 4), sd = round(sd(x), 4)))

  # Convert the aggregated results into a flat data frame
  resultsParametersGroup <- do.call(data.frame, resultsParametersGroup)

  # Rename columns dynamically for clarity
  colnames(resultsParametersGroup) <- c(grouping_columns, "mean", "sd")

  # Replace NA standard deviation values with 0
  resultsParametersGroup$sd <- ifelse(is.na(resultsParametersGroup$sd), 0, resultsParametersGroup$sd)

  #remove the calibration and the parameters files
  # Paths to the directories
  outputsCalib_dir <- file.path(config_folder, "outputsCalibration")
  outputsParameters_dir <- file.path(config_folder, "outputsParameters")

  # Remove all files in the directories
  if (dir.exists(outputsCalib_dir)) {
    file.remove(list.files(outputsCalib_dir, full.names = TRUE))
  }

  if (dir.exists(outputsParameters_dir)) {
    file.remove(list.files(outputsParameters_dir, full.names = TRUE))
  }

  #objects returned by the function
  return(list(calibration_results = results,
              parameters_pixels = resultsParameters,
              parameters_group = resultsParametersGroup))
}

#' Run SWELL Calibration in batch mode
#'
#' This function performs the calibration of the SWELL model using weather data as input and vegetation indices data (NDVI or EVI) as reference data using a multi-start simplex algorithm.
#' The function does not return any data structure. The last argument is the path where the .csv files for SWELL results and parameters will be saved.
#'
#' @param weather_data A data frame containing geographical and weather data.
#'   Columns must be: Latitude (numeric), Longitude (numeric), Date (YYYY-MM-DD), Tmin (numeric, °C), Tmax (numeric, °C).
#' @param vegetation_data A data frame containing the vegetation index data.
#'   Columns must be: PixelID (string), Group (string), Year (integer), Doy (integer), Longitude (numeric), Latitude (numeric), VegetationIndex (numeric).
#' @param vegetation_index The vegetation index used for SWELL calibration (string). Available options are 'EVI' and 'NDVI'.
#' @param SWELLparameters A nested list structured as `SWELLparameters$species$class$parameter`,
#'   containing SWELL model parameters. Each parameter is itself a list with the following elements:
#'   - `min`: Minimum value of the parameter (numeric), used for calibration.
#'   - `max`: Maximum value of the parameter (numeric), used for calibration.
#'   - `value`: Default value of the parameter (numeric).
#'   - `calibration`: Logical (`TRUE` if the parameter is under calibration, `FALSE` otherwise).
#' @param species the species to be used: it must be present in SWELLparameters as the first level  of the named list
#' @param start_year Start year for calibration (default: 2011).
#' @param end_year End year for calibration (default: 2022).
#' @param simplexes Number of simplexes for calibration (default: 3).
#' @param iterations Number of iterations for calibration (default: 1000).
#' @param outPath The output path for SWELL results and parameters
#' @return The function does not return any data structure
#' @export
swellCalibrationBatch <- function(weather_data, vegetation_data,
                             vegetationIndex = "EVI",
                             SWELLparameters, species,
                             start_year = 2011, end_year = 2022,
                             simplexes = 1, iterations = 1,
                             outPath=paste0(getwd())) {

  #### Input Validation ####
  # Check if outPath exists
  if (!file.exists(outPath)) {
    stop("The specified outputs path does not exist. Create it before running SWELL in batch mode")
  }

  # Check if vegetation_data is a data frame
  if (!is.data.frame(vegetation_data)) {
    stop("'vegetation_data' must be a data frame.")
  }

  # Check if vegetationIndex is valid
  if (!vegetationIndex %in% c("EVI", "NDVI")) {
    stop("'vegetationIndex' must be either 'EVI' or 'NDVI'.")
  }

  # Check if SWELLparameters is a named list
  if (!is.list(SWELLparameters) || is.null(names(SWELLparameters))) {
    stop("'SWELLparameters' must be a named list.")
  }

  # Check if start_year and end_year are numeric and valid
  if (!is.numeric(start_year) || !is.numeric(end_year)) {
    stop("'start_year' and 'end_year' must be numeric.")
  }
  if (start_year > end_year) {
    stop("'start_year' must be less than or equal to 'end_year'.")
  }

  # Check if 'Date' column exists in weather_data
  if (!"Date" %in% colnames(weather_data)) {
    stop("'weather_data' must contain a column named 'Date'.")
  }

  # Check if 'Date' column is of type Date
  if (!inherits(weather_data$Date, "Date")) {
    stop("'Date' column in 'weather_data' must be of type Date.")
  }

  # Check if start_year and end_year are present in the weather_data
  weather_years <- unique(format(weather_data$Date, "%Y"))
  if (!as.character(start_year) %in% weather_years) {
    stop(paste0("Start year ", start_year, " is not present in 'weather_data'."))
  }
  if (!as.character(end_year) %in% weather_years) {
    stop(paste0("End year ", end_year, " is not present in 'weather_data'."))
  }

  # Check if simplexes and iterations are positive integers
  if (!is.numeric(simplexes) || simplexes <= 0 || simplexes != as.integer(simplexes)) {
    stop("'simplexes' must be a positive integer.")
  }
  if (!is.numeric(iterations) || iterations <= 0 || iterations != as.integer(iterations)) {
    stop("'iterations' must be a positive integer.")
  }

  #### Path Configuration ####
  exe_path <- switch(Sys.info()["sysname"],
                     "Windows" = system.file("extdata", "Windows", "runner.exe", package = "SWELL"),
                     "Darwin"  = system.file("extdata", "macOS", "runner", package = "SWELL"),
                     "Linux"   = system.file("extdata", "Linux", "runner", package = "SWELL"))

  if (!file.exists(exe_path)) {
    stop("Executable not found at the specified path: ", exe_path)
  }

  config_folder <- dirname(exe_path)
  if (!dir.exists(config_folder)) {
    stop("Config folder does not exist: ", config_folder)
  }

  if (missing(config_folder)) {
    config_folder <- tempdir()
  }

  #### Prepare Weather Data ####
  weather_data$fileName <- paste0(weather_data$Latitude, "_", weather_data$Longitude)

  if ("Date" %in% names(weather_data)) {
    weather_data$Date <- gsub("^\"|\"$", "", as.character(weather_data$Date))
  }

  weather_dir <- file.path(config_folder, "weather")
  weather_file <- file.path(weather_dir, paste0(unique(weather_data$fileName), ".csv"))
  ndvi_file <- file.path(config_folder, "ndvi_data.csv")
  parameters_file <- file.path(config_folder, "parameters.csv")

  dir.create(weather_dir, showWarnings = FALSE, recursive = TRUE)
  dir.create(file.path(config_folder, "outputsValidation"), showWarnings = FALSE, recursive = TRUE)

  #### Write Weather Data ####
  unique_coords <- unique(weather_data[, c("Latitude", "Longitude")])

  for (i in seq_along(weather_file)) {
    file_name <- sub(".*/([^/]+)\\.csv$", "\\1", weather_file[i])
    filtered_data <- subset(weather_data, fileName == file_name)
    write.table(filtered_data, file = weather_file[i], sep = ",", row.names = FALSE, col.names = TRUE, quote = FALSE)
  }

  #### Write Vegetation Data ####
  write.table(vegetation_data, file = ndvi_file, sep = ",", row.names = FALSE, col.names = TRUE, quote = FALSE)

  SWELLparameters_df <- do.call(rbind, lapply(names(SWELLparameters), function(species) {
    do.call(rbind, lapply(names(SWELLparameters[[species]]), function(class) {
      do.call(rbind, lapply(names(SWELLparameters[[species]][[class]]), function(param) {
        param_list <- SWELLparameters[[species]][[class]][[param]]

        # Ensure numeric values are present and valid
        min_value <- ifelse(is.null(param_list$min), 0, param_list$min)
        max_value <- ifelse(is.null(param_list$max), 1, param_list$max)
        value <- ifelse(is.null(param_list$value), (min_value + max_value) / 2, param_list$value)
        calibration_flag <- ifelse(is.null(param_list$calibration), FALSE, param_list$calibration)

        data.frame(
          species = species,  # Include species column
          class = class,
          parameter = param,
          min = as.numeric(min_value),
          max = as.numeric(max_value),
          value = as.numeric(value),
          calibration = ifelse(calibration_flag, "x", "")  # Convert TRUE → "x", FALSE → ""
        )
      }))
    }))
  }))

  # Write the table to file
  write.table(SWELLparameters_df, file = parameters_file,
              sep = ",", row.names = FALSE, col.names = TRUE, quote = FALSE)


  # Prepare configuration file
  swell_config <- list(
    settings = list(
      calibration = "true",
      species = species,
      startYear = as.character(start_year),
      endYear = as.character(end_year),
      weatherDirectory = normalizePath(weather_dir),
      referenceDataFile = normalizePath(ndvi_file),
      parametersDataFile = normalizePath(parameters_file),
      validationReplicates = "1",
      parametersDistributions = "uniform",
      parametersValidationFile = "",
      simplexes = as.character(simplexes),
      iterations = as.character(iterations),
      vegetationIndex = vegetationIndex,
      outputCalibrationDir = normalizePath(outPath),
      outputValidationDir  = normalizePath(file.path(config_folder, "outputsValidation")),
      outputParametersDir  = normalizePath(outPath),
      startPixel = "0",
      numberPixels = "5000"
    )
  )

  # Save configuration to JSON
  json_file <- file.path(config_folder, "SWELLconfig.json")
  jsonlite::write_json(swell_config, json_file, auto_unbox = TRUE)

  # Execute runner program
  cmd <- paste(shQuote(exe_path), shQuote(json_file))
  message("Running command: ", cmd)
  result <- tryCatch({
    system(cmd, intern = FALSE)
    list(success = TRUE)
  }, error = function(e) {
    list(success = FALSE, error = e$message)
  })

  if (!result$success) {
    stop("Error while running executable: ", result$error)
  }

}

#' Run SWELL Validation
#'
#' This function performs the validation of the SWELL model using weather data, calibrated parameters, and vegetation index data (NDVI or EVI) as reference input.
#'
#' @param weather_data A data frame containing geographical and weather data.
#'   The following columns are required:
#'   \describe{
#'     \item{\code{Latitude}}{(numeric) Latitude of the location in decimal degrees.}
#'     \item{\code{Longitude}}{(numeric) Longitude of the location in decimal degrees.}
#'     \item{\code{Date}}{(Date) Date in \code{YYYY-MM-DD} format.}
#'     \item{\code{Tmin}}{(numeric) Minimum temperature (°C).}
#'     \item{\code{Tmax}}{(numeric) Maximum temperature (°C).}
#'   }
#'
#' @param vegetation_data A data frame containing the vegetation index data.
#'   The following columns are required:
#'   \describe{
#'     \item{\code{PixelID}}{(character) Unique identifier for a pixel or vegetation observation point.}
#'     \item{\code{Group}}{(character) Grouping variable (e.g., species, region).}
#'     \item{\code{Year}}{(integer) Year of the observation.}
#'     \item{\code{Doy}}{(integer) Day of the year.}
#'     \item{\code{Longitude}}{(numeric) Longitude of the pixel in decimal degrees.}
#'     \item{\code{Latitude}}{(numeric) Latitude of the pixel in decimal degrees.}
#'     \item{\code{VegetationIndex}}{(numeric) Value of the vegetation index (e.g., NDVI or EVI).}
#'   }
#'
#' @param vegetationIndex (string) The vegetation index used for SWELL calibration.
#'   Available options are \code{"EVI"} and \code{"NDVI"}.
#'
#' @param SWELLparameters A nested list structured as `SWELLparameters$species$class$parameter`,
#'   containing SWELL model parameters. Each parameter is itself a list with the following elements:
#'   - `min`: Minimum value of the parameter (numeric), used for calibration.
#'   - `max`: Maximum value of the parameter (numeric), used for calibration.
#'   - `value`: Default value of the parameter (numeric).
#'   - `calibration`: Logical (`TRUE` if the parameter is under calibration, `FALSE` otherwise).
#'
#' @param SWELLparametersCalibrated A data frame containing the calibrated SWELL model parameters, typically generated by the \code{swellCalibration} function.
#'   The following columns are required:
#'   \describe{
#'     \item{\code{species}}{(character) The species or vegetation type.}
#'     \item{\code{class}}{(character) The parameter class.}
#'     \item{\code{parameter}}{(character) The specific parameter name.}
#'     \item{\code{mean}}{(numeric) The mean value of the calibrated parameter.}
#'     \item{\code{sd}}{(numeric) The standard deviation of the calibrated parameter.}
#'   }
#' @param species the species to be used: it must be present in SWELLparameters as the first level  of the named list
#' @param parametersDistributions (string) The type of distribution used to sample SWELL parameters during validation.
#'   Available options are:
#'   \describe{
#'     \item{\code{"uniform"}}{Samples parameters using the mean ± \code{sd} as limits.}
#'     \item{\code{"normal"}}{Samples parameters using a normal distribution with the given \code{mean} and \code{sd}.}
#'   }
#'
#' @param start_year (integer) The starting year for the validation period.
#'   Default is \code{2011}.
#'
#' @param end_year (integer) The ending year for the validation period.
#'   Default is \code{2022}.
#'
#' @param validationReplicates (integer) The number of replicate simulations performed during validation.
#'   Default is \code{5}.
#'
#' @return A data frame containing the validation results.
#'   This data frame includes:
#'   \describe{
#'     \item{Weather data}{Key weather variables used in the SWELL model.}
#'     \item{Intermediate outputs}{SWELL outputs for each day and pixel.}
#'     \item{Simulated vegetation indices}{Median and percentile ranges (10th, 25th, 40th, 60th, 75th, and 90th) of the simulated vegetation indices.}
#'     \item{Reference data}{Observed vegetation index data (e.g., NDVI or EVI).}
#'   }
#'
#' @details This function validates the SWELL model by running it across the specified time period (\code{start_year} to \code{end_year}) using the provided weather and vegetation data. Calibration and sampling of parameters can be configured using the \code{SWELLparameters}, \code{SWELLparametersCalibrated}, and \code{parametersDistributions} arguments.
#'
#' @examples
#' \dontrun{
#' weather_data <- data.frame(
#'   Date = seq(as.Date("2010-01-01"), as.Date("2022-12-31"), by = "days"),
#'   Latitude = 45.0, Longitude = 7.5,
#'   Tmin = runif(4748, -5, 15), Tmax = runif(4748, 5, 30)
#' )
#' vegetation_data <- data.frame(
#'   PixelID = rep("pixel_1", 4748),
#'   Group = rep("group_1", 4748),
#'   Year = rep(2011:2022, each = 365),
#'   Doy = 1:365,
#'   Longitude = 7.5, Latitude = 45.0,
#'   VegetationIndex = runif(4748, 0, 1)
#' )
#' SWELLparameters <- data.frame(
#'   species = "beech", class = "parDormancy",
#'   parameter = "limitingPhotoperiod", min = 12.7, max = 13.5, value = 13.0,
#'   is_calibrated = FALSE
#' )
#' SWELLparametersCalibrated <- data.frame(
#'   species = "beech", class = "parDormancy",
#'   parameter = "limitingPhotoperiod", mean = 13.0, sd = 0.1
#' )
#' swellValidation(
#'   weather_data, vegetation_data, vegetationIndex = "EVI",
#'   SWELLparameters, SWELLparametersCalibrated,
#'   start_year = 2011, end_year = 2022, validationReplicates = 5
#' )
#' }
#' @export

swellValidation <- function(weather_data, vegetation_data, vegetationIndex = "EVI",
                            SWELLparameters,
                            SWELLparametersCalibrated, species = 'beech',
                            start_year = 2011, end_year = 2022,
                            validationReplicates = 5) {

  ####input check####
  # Check if weather_data is a data frame
  if (!is.data.frame(weather_data)) {
    stop("'weather_data' must be a data frame.")
  }

  # Check if vegetation_data is a data frame
  if (!is.data.frame(vegetation_data)) {
    stop("'vegetation_data' must be a data frame.")
  }

  # Check if vegetationIndex is one of the allowed values
  if (!vegetationIndex %in% c("EVI", "NDVI")) {
    stop("'vegetationIndex' must be either 'EVI' or 'NDVI'.")
  }


  # Check if SWELLparameters is a named list
  if (!is.list(SWELLparameters) || is.null(names(SWELLparameters))) {
    stop("'SWELLparameters' must be a named list.")
  }

  # Check if SWELLparametersCalibrated is a list
  if (!is.data.frame(SWELLparametersCalibrated)) {
    stop("'SWELLparametersCalibrated' must be dataframe.")
  }

  # Check if start_year and end_year are numeric and valid
  if (!is.numeric(start_year) || !is.numeric(end_year)) {
    stop("'start_year' and 'end_year' must be numeric.")
  }
  if (start_year > end_year) {
    stop("'start_year' must be less than or equal to 'end_year'.")
  }

  # Check if 'Date' column exists in weather_data
  if (!"Date" %in% colnames(weather_data)) {
    stop("'weather_data' must contain a column named 'Date'.")
  }

  # Check if 'Date' column is of type Date
  if (!inherits(weather_data$Date, "Date")) {
    stop("'Date' column in 'weather_data' must be of type Date.")
  }

  # Check if start_year and end_year are present in the weather_data
  weather_years <- unique(format(weather_data$Date, "%Y"))
  if (!as.character(start_year) %in% weather_years) {
    stop(paste0("Start year ", start_year, " is not present in 'weather_data'."))
  }
  if (!as.character(end_year) %in% weather_years) {
    stop(paste0("End year ", end_year, " is not present in 'weather_data'."))
  }
  if (!is.numeric(validationReplicates) || validationReplicates <= 0 || validationReplicates != as.integer(validationReplicates)) {
    stop("'validationReplicates' must be a positive integer.")
  }



  #####Determine executable path based on the operating system####
  exe_path <- switch(Sys.info()["sysname"],
                     "Windows" =
                       system.file("extdata", "Windows", "runner.exe", package = "SWELL"),
                     "Darwin"  =
                       system.file("extdata", "macOS", "runner", package = "SWELL"),
                     "Linux"   =
                       system.file("extdata", "Linux", "runner", package = "SWELL"))

  if (!file.exists(exe_path)) {
    stop("Executable not found at the specified path: ", exe_path)
  }


  # Define configuration folder and ensure it exists
  config_folder <- dirname(exe_path)
  if (!dir.exists(config_folder)) {
    stop("Config folder does not exist: ", config_folder)
  }

  # Default config_folder to a suitable directory
  if (missing(config_folder)) {
    config_folder <- tempdir()  # Use a temporary directory by default
  }

  # Prepare weather data with unique file names
  weather_data$fileName <- paste0(weather_data$Latitude, "_", weather_data$Longitude)

  # Clean date column if present
  if ("Date" %in% names(weather_data)) {
    weather_data$Date <- gsub("^\"|\"$", "", as.character(weather_data$Date))
  }

  # Define file paths
  weather_dir <- file.path(config_folder, "weather")
  weather_file <- file.path(weather_dir, paste0(unique(weather_data$fileName), ".csv"))
  ndvi_file <- file.path(config_folder, "ndvi_data.csv")
  parameters_file <- file.path(config_folder, "parameters.csv")

  write.csv(SWELLparametersCalibrated |> select(1,3:4,7,8),
            file.path(config_folder, "parametersCalibrated.csv"),
            row.names = FALSE, quote = FALSE)
  parametersCalib <- file.path(config_folder, "parametersCalibrated.csv")

  # Ensure necessary directories exist
  dir.create(weather_dir, showWarnings = FALSE, recursive = TRUE)
  dir.create(file.path(config_folder, "outputsCalibration"), showWarnings = FALSE, recursive = TRUE)
  dir.create(file.path(config_folder, "outputsValidation"), showWarnings = FALSE, recursive = TRUE)
  dir.create(file.path(config_folder, "outputsParameters"), showWarnings = FALSE, recursive = TRUE)

  # Write data files without quotes around values
  write.table(weather_data, file = weather_file, sep = ",", row.names = FALSE, col.names = TRUE, quote = FALSE)
  write.table(vegetation_data, file = ndvi_file, sep = ",", row.names = FALSE, col.names = TRUE, quote = FALSE)

  SWELLparameters_df <- do.call(rbind, lapply(names(SWELLparameters), function(species) {
    do.call(rbind, lapply(names(SWELLparameters[[species]]), function(class) {
      do.call(rbind, lapply(names(SWELLparameters[[species]][[class]]), function(param) {
        param_list <- SWELLparameters[[species]][[class]][[param]]

        # Ensure numeric values are present and valid
        min_value <- ifelse(is.null(param_list$min), 0, param_list$min)
        max_value <- ifelse(is.null(param_list$max), 1, param_list$max)
        value <- ifelse(is.null(param_list$value), (min_value + max_value) / 2, param_list$value)
        calibration_flag <- ifelse(is.null(param_list$calibration), FALSE, param_list$calibration)

        data.frame(
          species = species,  # Include species column
          class = class,
          parameter = param,
          min = as.numeric(min_value),
          max = as.numeric(max_value),
          value = as.numeric(value),
          calibration = ifelse(calibration_flag, "x", "")  # Convert TRUE → "x", FALSE → ""
        )
      }))
    }))
  }))

  # Write the table to file
  write.table(SWELLparameters_df,
              file = parameters_file, sep = ",", row.names = FALSE, col.names = TRUE, quote = FALSE)


  # Prepare configuration file
  swell_config <- list(
    settings = list(
      calibration = "false",
      species = species,
      startYear = as.character(start_year),
      endYear = as.character(end_year),
      weatherDirectory = normalizePath(weather_dir),
      referenceDataFile = normalizePath(ndvi_file),
      parametersDataFile = normalizePath(parameters_file),
      validationReplicates = validationReplicates,
      parametersDistributions = "uniform",
      parametersValidationFile = normalizePath(parametersCalib),
      simplexes = as.character(1),
      iterations = as.character(1),
      vegetationIndex = vegetationIndex,
      outputCalibrationDir = normalizePath(file.path(config_folder, "outputsCalibration")),
      outputValidationDir = normalizePath(file.path(config_folder, "outputsValidation")),
      outputParametersDir = normalizePath(file.path(config_folder, "outputsParameters")),
      startPixel = "0",
      numberPixels = "5000"
    )
  )

  # Save configuration to JSON
  json_file <- file.path(config_folder, "SWELLconfig.json")
  jsonlite::write_json(swell_config, json_file, auto_unbox = TRUE)

  # Execute runner program
  cmd <- paste(shQuote(exe_path), shQuote(json_file))
  message("Running command: ", cmd)
  result <- tryCatch({
    system(cmd, intern = FALSE)
    list(success = TRUE)
  }, error = function(e) {
    list(success = FALSE, error = e$message)
  })

  if (!result$success) {
    stop("Error while running executable: ", result$error)
  }

  # Load calibration results
  output_dir <- file.path(config_folder, "outputsValidation")
  csv_files <- list.files(output_dir, pattern = "\\.csv$", full.names = TRUE)
  if (length(csv_files) == 0) {
    stop("No CSV files found in the directory: ", output_dir)
  }

  # Combine results into a single data frame
  results <- do.call(rbind, lapply(csv_files, read.csv, stringsAsFactors = FALSE))
  # Update rates in results data frame
  results$date<-as.Date(results$date, format = "%m/%d/%Y")
  results$dormancyInductionRate <- ifelse(results$dormancyPercentage == 100, 0, results$dormancyInductionRate)
  results$endodormancyRate <- ifelse(results$dormancyInductionRate == 0, results$endodormancyRate, 0)
  results$growthRate <- ifelse(results$ecodormancyRate == 0, results$growthRate, 0)
  results$greendownRate <- ifelse(results$growthRate > 0 | results$declineRate > 0, 0, results$greendownRate)
  results$dormancyInductionRate <- ifelse(results$greendownRate > 0, 0, results$dormancyInductionRate)

  # Convert the aggregated results into a flat dataframe
  #remove the calibration and the parameters files
  # Paths to the directories
  outputsValid_dir <- file.path(config_folder, "outputsValidation")

  # Remove all files in the directories
  if (dir.exists(outputsValid_dir)) {
    file.remove(list.files(outputsValid_dir, full.names = TRUE))
  }

  #objects returned by the function
  return(results)
}



#' Run SWELL Validation
#'
#' This function performs the validation of the SWELL model using weather data, calibrated parameters, and vegetation index data (NDVI or EVI) as reference input.
#'
#' @param weather_data A data frame containing geographical and weather data.
#'   The following columns are required:
#'   \describe{
#'     \item{\code{Latitude}}{(numeric) Latitude of the location in decimal degrees.}
#'     \item{\code{Longitude}}{(numeric) Longitude of the location in decimal degrees.}
#'     \item{\code{Date}}{(Date) Date in \code{YYYY-MM-DD} format.}
#'     \item{\code{Tmin}}{(numeric) Minimum temperature (°C).}
#'     \item{\code{Tmax}}{(numeric) Maximum temperature (°C).}
#'   }
#'
#' @param vegetation_data A data frame containing the vegetation index data.
#'   The following columns are required:
#'   \describe{
#'     \item{\code{PixelID}}{(character) Unique identifier for a pixel or vegetation observation point.}
#'     \item{\code{Group}}{(character) Grouping variable (e.g., species, region).}
#'     \item{\code{Year}}{(integer) Year of the observation.}
#'     \item{\code{Doy}}{(integer) Day of the year.}
#'     \item{\code{Longitude}}{(numeric) Longitude of the pixel in decimal degrees.}
#'     \item{\code{Latitude}}{(numeric) Latitude of the pixel in decimal degrees.}
#'     \item{\code{VegetationIndex}}{(numeric) Value of the vegetation index (e.g., NDVI or EVI).}
#'   }
#'
#' @param vegetationIndex (string) The vegetation index used for SWELL calibration.
#'   Available options are \code{"EVI"} and \code{"NDVI"}.
#'
#' @param SWELLparameters A nested list structured as `SWELLparameters$species$class$parameter`,
#'   containing SWELL model parameters. Each parameter is itself a list with the following elements:
#'   - `min`: Minimum value of the parameter (numeric), used for calibration.
#'   - `max`: Maximum value of the parameter (numeric), used for calibration.
#'   - `value`: Default value of the parameter (numeric).
#'   - `calibration`: Logical (`TRUE` if the parameter is under calibration, `FALSE` otherwise).
#'
#' @param SWELLparametersCalibrated A data frame containing the calibrated SWELL model parameters, typically generated by the \code{swellCalibration} function.
#'   The following columns are required:
#'   \describe{
#'     \item{\code{species}}{(character) The species or vegetation type.}
#'     \item{\code{class}}{(character) The parameter class.}
#'     \item{\code{parameter}}{(character) The specific parameter name.}
#'     \item{\code{mean}}{(numeric) The mean value of the calibrated parameter.}
#'     \item{\code{sd}}{(numeric) The standard deviation of the calibrated parameter.}
#'   }
#' @param species the species to be used: it must be present in SWELLparameters as the first level  of the named list
#' @param parametersDistributions (string) The type of distribution used to sample SWELL parameters during validation.
#'   Available options are:
#'   \describe{
#'     \item{\code{"uniform"}}{Samples parameters using the mean ± \code{sd} as limits.}
#'     \item{\code{"normal"}}{Samples parameters using a normal distribution with the given \code{mean} and \code{sd}.}
#'   }
#'
#' @param start_year (integer) The starting year for the validation period.
#'   Default is \code{2011}.
#'
#' @param end_year (integer) The ending year for the validation period.
#'   Default is \code{2022}.
#'
#' @param validationReplicates (integer) The number of replicate simulations performed during validation.
#'   Default is \code{5}.
#'
#' @return A data frame containing the validation results.
#'   This data frame includes:
#'   \describe{
#'     \item{Weather data}{Key weather variables used in the SWELL model.}
#'     \item{Intermediate outputs}{SWELL outputs for each day and pixel.}
#'     \item{Simulated vegetation indices}{Median and percentile ranges (10th, 25th, 40th, 60th, 75th, and 90th) of the simulated vegetation indices.}
#'     \item{Reference data}{Observed vegetation index data (e.g., NDVI or EVI).}
#'   }
#'
#' @details This function validates the SWELL model by running it across the specified time period (\code{start_year} to \code{end_year}) using the provided weather and vegetation data. Calibration and sampling of parameters can be configured using the \code{SWELLparameters}, \code{SWELLparametersCalibrated}, and \code{parametersDistributions} arguments.
#'
#' @examples
#' \dontrun{
#' weather_data <- data.frame(
#'   Date = seq(as.Date("2010-01-01"), as.Date("2022-12-31"), by = "days"),
#'   Latitude = 45.0, Longitude = 7.5,
#'   Tmin = runif(4748, -5, 15), Tmax = runif(4748, 5, 30)
#' )
#' vegetation_data <- data.frame(
#'   PixelID = rep("pixel_1", 4748),
#'   Group = rep("group_1", 4748),
#'   Year = rep(2011:2022, each = 365),
#'   Doy = 1:365,
#'   Longitude = 7.5, Latitude = 45.0,
#'   VegetationIndex = runif(4748, 0, 1)
#' )
#' SWELLparameters <- data.frame(
#'   species = "beech", class = "parDormancy",
#'   parameter = "limitingPhotoperiod", min = 12.7, max = 13.5, value = 13.0,
#'   is_calibrated = FALSE
#' )
#' SWELLparametersCalibrated <- data.frame(
#'   species = "beech", class = "parDormancy",
#'   parameter = "limitingPhotoperiod", mean = 13.0, sd = 0.1
#' )
#' swellValidation(
#'   weather_data, vegetation_data, vegetationIndex = "EVI",
#'   SWELLparameters, SWELLparametersCalibrated,
#'   start_year = 2011, end_year = 2022, validationReplicates = 5
#' )
#' }
#' @export

swellValidationBatch <- function(weather_data, vegetation_data, vegetationIndex = "EVI",
                            SWELLparameters,
                            SWELLparametersCalibrated, species = 'beech',
                            start_year = 2011, end_year = 2022,
                            validationReplicates = 5,
                            outPath=paste0(getwd())) {
  
  ####input check####
  # Check if weather_data is a data frame
  if (!is.data.frame(weather_data)) {
    stop("'weather_data' must be a data frame.")
  }
  
  # Check if vegetation_data is a data frame
  if (!is.data.frame(vegetation_data)) {
    stop("'vegetation_data' must be a data frame.")
  }
  
  # Check if vegetationIndex is one of the allowed values
  if (!vegetationIndex %in% c("EVI", "NDVI")) {
    stop("'vegetationIndex' must be either 'EVI' or 'NDVI'.")
  }
  
  
  # Check if SWELLparameters is a named list
  if (!is.list(SWELLparameters) || is.null(names(SWELLparameters))) {
    stop("'SWELLparameters' must be a named list.")
  }
  
  # Check if SWELLparametersCalibrated is a list
  if (!is.data.frame(SWELLparametersCalibrated)) {
    stop("'SWELLparametersCalibrated' must be dataframe.")
  }
  
  # Check if start_year and end_year are numeric and valid
  if (!is.numeric(start_year) || !is.numeric(end_year)) {
    stop("'start_year' and 'end_year' must be numeric.")
  }
  if (start_year > end_year) {
    stop("'start_year' must be less than or equal to 'end_year'.")
  }
  
  # Check if 'Date' column exists in weather_data
  if (!"Date" %in% colnames(weather_data)) {
    stop("'weather_data' must contain a column named 'Date'.")
  }
  
  # Check if 'Date' column is of type Date
  if (!inherits(weather_data$Date, "Date")) {
    stop("'Date' column in 'weather_data' must be of type Date.")
  }
  
  # Check if start_year and end_year are present in the weather_data
  weather_years <- unique(format(weather_data$Date, "%Y"))
  if (!as.character(start_year) %in% weather_years) {
    stop(paste0("Start year ", start_year, " is not present in 'weather_data'."))
  }
  if (!as.character(end_year) %in% weather_years) {
    stop(paste0("End year ", end_year, " is not present in 'weather_data'."))
  }
  if (!is.numeric(validationReplicates) || validationReplicates <= 0 || validationReplicates != as.integer(validationReplicates)) {
    stop("'validationReplicates' must be a positive integer.")
  }
  
  
  
  #####Determine executable path based on the operating system####
  exe_path <- switch(Sys.info()["sysname"],
                     "Windows" =
                       system.file("extdata", "Windows", "runner.exe", package = "SWELL"),
                     "Darwin"  =
                       system.file("extdata", "macOS", "runner", package = "SWELL"),
                     "Linux"   =
                       system.file("extdata", "Linux", "runner", package = "SWELL"))
  
  if (!file.exists(exe_path)) {
    stop("Executable not found at the specified path: ", exe_path)
  }
  
  
  # Define configuration folder and ensure it exists
  config_folder <- dirname(exe_path)
  if (!dir.exists(config_folder)) {
    stop("Config folder does not exist: ", config_folder)
  }
  
  # Default config_folder to a suitable directory
  if (missing(config_folder)) {
    config_folder <- tempdir()  # Use a temporary directory by default
  }
  
  # Prepare weather data with unique file names
  weather_data$fileName <- paste0(weather_data$Latitude, "_", weather_data$Longitude)
  
  # Clean date column if present
  if ("Date" %in% names(weather_data)) {
    weather_data$Date <- gsub("^\"|\"$", "", as.character(weather_data$Date))
  }
  
  # Define file paths
  weather_dir <- file.path(config_folder, "weather")
  weather_file <- file.path(weather_dir, paste0(unique(weather_data$fileName), ".csv"))
  ndvi_file <- file.path(config_folder, "ndvi_data.csv")
  parameters_file <- file.path(config_folder, "parameters.csv")
  
  write.csv(SWELLparametersCalibrated |> select(1,3:4,7,8),
            file.path(config_folder, "parametersCalibrated.csv"),
            row.names = FALSE, quote = FALSE)
  parametersCalib <- file.path(config_folder, "parametersCalibrated.csv")
  
  # Ensure necessary directories exist
  dir.create(weather_dir, showWarnings = FALSE, recursive = TRUE)
  dir.create(file.path(config_folder, "outputsCalibration"), showWarnings = FALSE, recursive = TRUE)
  dir.create(file.path(config_folder, "outputsValidation"), showWarnings = FALSE, recursive = TRUE)
  dir.create(file.path(config_folder, "outputsParameters"), showWarnings = FALSE, recursive = TRUE)
  
  # Write data files without quotes around values
  write.table(weather_data, file = weather_file, sep = ",", row.names = FALSE, col.names = TRUE, quote = FALSE)
  write.table(vegetation_data, file = ndvi_file, sep = ",", row.names = FALSE, col.names = TRUE, quote = FALSE)
  
  SWELLparameters_df <- do.call(rbind, lapply(names(SWELLparameters), function(species) {
    do.call(rbind, lapply(names(SWELLparameters[[species]]), function(class) {
      do.call(rbind, lapply(names(SWELLparameters[[species]][[class]]), function(param) {
        param_list <- SWELLparameters[[species]][[class]][[param]]
        
        # Ensure numeric values are present and valid
        min_value <- ifelse(is.null(param_list$min), 0, param_list$min)
        max_value <- ifelse(is.null(param_list$max), 1, param_list$max)
        value <- ifelse(is.null(param_list$value), (min_value + max_value) / 2, param_list$value)
        calibration_flag <- ifelse(is.null(param_list$calibration), FALSE, param_list$calibration)
        
        data.frame(
          species = species,  # Include species column
          class = class,
          parameter = param,
          min = as.numeric(min_value),
          max = as.numeric(max_value),
          value = as.numeric(value),
          calibration = ifelse(calibration_flag, "x", "")  # Convert TRUE → "x", FALSE → ""
        )
      }))
    }))
  }))
  
  # Write the table to file
  write.table(SWELLparameters_df,
              file = parameters_file, sep = ",", row.names = FALSE, col.names = TRUE, quote = FALSE)
  
  
  # Prepare configuration file
  swell_config <- list(
    settings = list(
      calibration = "false",
      species = species,
      startYear = as.character(start_year),
      endYear = as.character(end_year),
      weatherDirectory = normalizePath(weather_dir),
      referenceDataFile = normalizePath(ndvi_file),
      parametersDataFile = normalizePath(parameters_file),
      validationReplicates = validationReplicates,
      parametersDistributions = "uniform",
      parametersValidationFile = normalizePath(parametersCalib),
      simplexes = as.character(1),
      iterations = as.character(1),
      vegetationIndex = vegetationIndex,
      outputCalibrationDir = normalizePath(file.path(config_folder, "outputsCalibration")),
      outputValidationDir = outPath,
      outputParametersDir = normalizePath(file.path(config_folder, "outputsParameters")),
      startPixel = "0",
      numberPixels = "5000"
    )
  )
  
  # Save configuration to JSON
  json_file <- file.path(config_folder, "SWELLconfig.json")
  jsonlite::write_json(swell_config, json_file, auto_unbox = TRUE)
  
  # Execute runner program
  cmd <- paste(shQuote(exe_path), shQuote(json_file))
  message("Running command: ", cmd)
  result <- tryCatch({
    system(cmd, intern = FALSE)
    list(success = TRUE)
  }, error = function(e) {
    list(success = FALSE, error = e$message)
  })
  
  if (!result$success) {
    stop("Error while running executable: ", result$error)
  }
}

