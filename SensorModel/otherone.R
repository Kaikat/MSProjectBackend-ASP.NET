AirstripAllData <- read.delim("AirstripALL_table1.txt", header = TRUE)
airstrip <- read.csv("AirstripALL_table1.csv")

#Removing uneeded columns
NewAirData <- subset(AirstripAllData, select = -c(52:101))
ct = -c(29,55))

NewAirData1 <- AirstripAllData[,1:51]
colnames(NewAirData1) <- apply(NewAirData1[1,], c(1), function(x) as.character(x))
NewAirData1 <- NewAirData1[-1,]

#Removing N/A data

New2Air <- NewAirData[-1:-4,]
New3Air <- New2Air[New2Air$X9910 != 'NAN',]
New4Air <- New3Air[,-(1:3)]
New5Air <- New4Air[,-which(names(New4Air) %in% c("Table1","X","X.2","X.3","X.5","X.6","X.9","X.10","X.12","X.13","X.14","X.15","X.19","X.20","X.21","X.23","X.24","X.28","X.29","X.30","X.31","X.32","X.33","X.35","X.38","X.39","X.41","X.42"))]
New6Air <- New5Air[New5Air$X.26 != 'NAN',]
New7Air <- subset(New6Air, select = -(2:3))
New8Air <- New7Air[1:245525,]
TestAir <- New7Air[1:200,1:5]

kk <- lm(TestAir$X9910~TestAir$X59799)

plm <- as.vector.factor(TestAir$X9910)
ggg <- as.vector.factor(TestAir$X59799)
hhh <- lm(plm~ggg)
summary(hhh)
plot(hhh)

shit <- as.numeric(TestAir$X9910)
test = as.numeric(TestAir$X59799)
model1 <- lm(test~shit)
model1
summary(model1)
hist(shit)
