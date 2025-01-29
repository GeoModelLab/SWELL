#' SWELL Parameters Data Used in Calibration
#'
#' A nested list providing default values for the SWELL model to reproduce tree phenology and vegetation dynamics.
#' Users can modify this dataset as needed before using it in the `swellCalibration` function for calibration.
#'
#' @format A nested list structured as `SWELLparameters$species$class$parameter`, where:
#' \describe{
#'   \item{species}{The tree species for which parameters apply (e.g., "beech").}
#'   \item{class}{A category grouping related parameters (e.g., "parDormancy", "parGrowth").}
#'   \item{parameter}{A specific parameter within a class (e.g., "limitingPhotoperiod", "thermalThreshold").}
#'   \item{min}{Minimum value of the parameter (numeric). Used for automatic calibration if `calibration` is TRUE.}
#'   \item{max}{Maximum value of the parameter (numeric). Used for automatic calibration if `calibration` is TRUE.}
#'   \item{value}{Default value of the parameter (numeric).}
#'   \item{calibration}{Logical, TRUE if the parameter is under calibration, FALSE otherwise.}
#' }
#'
#' @details This dataset is structured as a hierarchical list to facilitate easy access to SWELL parameters.
#' Users can retrieve or modify specific parameter values using expressions like:
#' \code{SWELLparameters$beech$parDormancy$limitingPhotoperiod$min}.
#' The dataset serves as the starting point for parameter values in the `swellCalibration` function.
#' Users can edit or replace parameter values to suit specific modeling needs or calibration goals.
#'
#' @source <https://github.com/GeoModelLab/SWELL/tree/main/data>
"SWELLparameters"

