airstrip <- read.csv("AirstripALL_table1_modified.csv", header = TRUE)

# Prune for NAN, NA
airstripNoNan <- airstrip[airstrip$TC != 'NAN',]
airstripNoNan$TC <- as.numeric(as.character(airstripNoNan$TC))
airstripNoNa <- airstripNoNan[!is.na(airstripNoNan$TC),]
attach(airstripNoNa)

meanTC <- mean(TC)
stddevTC <- sd(TC)

# Given the temperature and normal distribution parameters for TC,
# calculate an "arbitrary" health indicator
tempToHealth <- function(temp, mu, sigma) {
  tScore <- (temp - mu)/sigma
  # how calculate probability i guess?
  # because TC is normal, we're more-or-less just trying to map TC's
  # "range" to a health factors' range of (0,100)
  # 
  health <- (1 - abs(tScore)/3)*100
  return(health)
}

testHF <- tempToHealth(10, mean(TC), sqrt(var(TC)))



HFcol <- unlist(lapply(TC, function(t) tempToHealth(t, meanTC, stddevTC)))

airstripWithHealth <- airstripNoNa
airstripWithHealth$HF <- HFcol

fit <- lm(airstripWithHealth$HF ~ airstripWithHealth$TC)
summary(fit)
