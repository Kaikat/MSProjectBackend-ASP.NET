AirstripAllData <- read.delim("AirstripALL_table1.txt", header = TRUE)
CoprAllData <- read.delim("CoprALL_table1.csv.txt", header = TRUE)
LisqueAllData <- read.delim("LisqueALL_table1.txt", header = TRUE)
McaAllData <- read.delim("McaALL_table1.txt", header = TRUE)
PACAALLData <- read.delim("PACAALL_table1.txt", header = TRUE)

#Removing uneeded columns
NewAirData <- subset(AirstripAllData, select = -c(52:101))
NewCoprData <- subset(CoprAllData, select = -c(51:99))
NewLisqueData <- subset(LisqueAllData, select = -c(37:71))
NewMcaData <- subset(McaAllData, select = -c(29,55))
NewPACAData <- subset(PACAALLData)

#Removing N/A data

New2Air <- NewAirData[-1:-4,]
New3Air <- New2Air[New2Air$X9910 != 'NAN',]
New4Air <- New3Air[,-(1:3)]
New5Air <- New4Air[,-which(names(New4Air) %in% c("Table1","X","X.2","X.3","X.5","X.6","X.9","X.10","X.12","X.13","X.14","X.15","X.19","X.20","X.21","X.23","X.24","X.28","X.29","X.30","X.31","X.32","X.33","X.35","X.38","X.39","X.41","X.42"))]
New6Air <- New5Air[New5Air$X.26 != 'NAN',]
New7Air <- subset(New6Air, select = -(2:3))
New8Air <- New7Air[1:245525,]
TestAir <- New7Air[1:200,1:5]



shit <- as.numeric(TestAir$X.1)
test = as.numeric(TestAir$X9910)
model1 <- lm(test~shit)
model1
summary(model1)
