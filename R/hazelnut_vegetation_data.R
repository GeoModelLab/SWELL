#' Vegetation data used for the vignette on SWELL calibration using NDVI and EVI from hazelnut orchards
#'
#' A dataframe providing MODIS NDVI and EVI data from 2009 to 2023 on 20 hazelnut orchards.
#'
#' @format A data frame with 13000 rows and 8 columns:
#' \describe{
#'   \item{orchard_id}{id of the hazelnut orchard}
#'   \item{Municipality}{Municipality (NUTS 3 unit level)}
#'   \item{latitude}{Latitude (numeric)}
#'   \item{longitude}{Longitude (numeric)}
#'   \item{year}{Year (numeric)}
#'   \item{doy}{Day of the year (numeric).}
#'   \item{ndvi}{Normalized Difference Vegetation Index (numeric)}
#'   \item{evi}{Enhanced Vegetation Index (numeric)}
#' }
#'
#' @details This dataset is used as the data source for the vignette on SWELL calibration.
#'
#' @source <https://www.who.int/teams/global-tuberculosis-programme/data>
"hazelnut_vegetation_data"
