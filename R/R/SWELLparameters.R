#' SWELL parameters data used in calibration
#'
#' A dataframe providing default values for SWELL application to reproduce beech vegetation dynamics.
#' Users can modify this dataset as needed before using it in the `swellCalibration` function for calibration.
#'
#' @format A data frame with 31 rows and 7 columns:
#' \describe{
#'   \item{species}{Species name.}
#'   \item{class}{Class of the parameter.}
#'   \item{parameter}{Name of the parameter.}
#'   \item{min}{Minimum value of the parameter (numeric). Used for automatic calibration if `calibration` is TRUE.}
#'   \item{max}{Maximum value of the parameter (numeric). Used for automatic calibration if `calibration` is TRUE.}
#'   \item{value}{Default value of the parameter (numeric).}
#'   \item{calibration}{Logical, TRUE if the parameter is under calibration, FALSE otherwise.}
#' }
#'
#' @details This dataset is used as the starting point for parameter values in the `swellCalibration` function.
#' Users can edit or replace parameter values to suit specific modeling needs or calibration goals.
#'
#' @source <https://www.who.int/teams/global-tuberculosis-programme/data>
"SWELLparameters"
