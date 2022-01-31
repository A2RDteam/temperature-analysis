library(ggplot2)
library(viridis)
#library(RColorBrewer)
library("ggsci")
#library(hash)
library('Cairo')
library(lubridate)

#lattiutudes = c( 77.99, 73.485, 70.61, 55.956, -2.088)
#longitudes = c(15.12, 80.828, -159.8859, 107.6825, -65.8868)
# pointNames = c("Шпицберген", "Таймыр", "Аляска", "Прибайкалье", "Верховья Амазонки", "Якутия")
pointNames = c("Svalbard", "Taimyr", "Alaska", "Northern Baikal", "Upper Amazon", "Yakutia")

#monthNames <- hash()

CairoWin()
zerov<-c(0)
for(point in 1:length(pointNames)){
#for(point in 1:1){
  
  yearAvg <- array(data = zerov, dim = c(120, 12))
  rollMeanYear <- array(data = zerov, dim = c(120, 12))
  for(mon in 1:12){
    # filename <- "33_68_month_0.csv"
    filename <- paste(pointNames[point], "_", month.name[mon], ".csv",sep = "")
    # read onepoint time-data
    data <- read.csv(filename, header = FALSE, sep = ';', dec = '.')
    # add years values
    data = cbind(data, 1901:2020)
    #rmean <- rollmean(data[1], 30)
    # add rolling mean. width 1 to 30 (smaller for first 29 rows)
    r_mean <-c()
    for(id in 1:120){
      if(id <= 30){
        r_mean[id] <- mean(data$V1[1:id])
      }
      else{
        r_mean[id] <- mean(data$V1[(1+id-30):id])
      }
    }
    data = cbind(data, r_mean)
    # add difference fact - rolling mean
    diff <- data$V1 - data$r_mean
    data = cbind(data, diff)
    # add cumulative average
    c_avg <- c()
    for(id in 1:120){
      c_avg[id] <- mean(data$V1[1:id])
    }
    data = cbind(data, c_avg)
    # add cumulative difference
    prev_dif <- 0
    c_dif <- c()
    c_dif[1] <- prev_dif
    for(id in 2:120){
      c_dif[id] <- prev_dif + data$diff[id]
      prev_dif = c_dif[id]
    }
    data = cbind(data, c_dif)
    # add difference fact-overall mean
    avg_diff <- data$V1 - mean(data$V1)
    data = cbind(data, avg_diff)
    # average for 1961-1990
    sum<-0
    for(id in 61:90){
      sum = sum + data$V1[id]
    }
    average30y = sum / 30
    # diff: moving average - cumulative average
    diff_tr12 <- data$r_mean - data$c_avg
    data = cbind(data, diff_tr12)
    # diff: moving average - average 1961-1990
    diff_tr2ave30 <- data$r_mean - average30y
    data = cbind(data, diff_tr2ave30)
    # calc year average
    
    for(id in 1:120){
      if(mon < 10)
        date_formate <- as.Date(paste(1900+id, "-0", mon, "-01", sep = ""), "%Y-%m-%d")
      else
        date_formate <- as.Date(paste(1900+id, "-", mon, "-01", sep = ""), "%Y-%m-%d")
      days <- days_in_month(date_formate)[[1]]
      yearAvg[id, mon] <- days * data$V1[id]
    }
    #make names for columns
    colnames(data) <- c("Temp","Year","Rollmean","RmeanDif","CumAvg","CumDif","AvgDif", "Diff2trends", "DiffTrend30y")
    minLimit <- min(data$Temp)
    maxLimit <- minLimit + 15
    # make a plot
    plot <- ggplot(data = data)+
      geom_line(aes(x=Year, y=Temp, colour="1.Temperature"), size=1, linetype="dotted")+
      geom_line(aes(x=Year, y=CumAvg, colour="2.Cumulative average"), size=1.2)+
      geom_line(aes(x=Year, y=Rollmean, colour="3.Moving average"), size=1.2)+
      geom_hline(aes(yintercept = average30y, colour="4. 1961-1990 average"), size=1.2, linetype="dashed")+
      
      #geom_line(aes(x=Year, y=RmeanDif, colour="3.Rmean difference"), size=1.5)+
      
      #geom_line(aes(x=Year, y=CumDif, colour="5.Cumulative difference"), size=1.5)+
      #geom_line(aes(x=Year, y=AvgDif, colour="5.Average difference"), size=1.5)+
      # ylim(minLimit, maxLimit)+
      scale_color_npg(name="Variables" )+
      ggtitle(paste("Temperature in '",pointNames[point],"' ",month.name[mon]," for 120 years", sep='') )+
      ylab("Degrees")+
      coord_cartesian(xlim=c(1901, 2022), expand = FALSE)+
      scale_x_continuous(minor_breaks = seq(1901, 2021, 1), breaks = seq(1901, 2021, 10))+
      scale_y_continuous(breaks = seq(minLimit, maxLimit, 1))+
      theme(legend.position="top", legend.text=element_text(size=12), legend.title=element_text(size=10))
    # filename <- paste(toString(as.integer(lattiutudes[point])), "_", toString(as.integer(longitudes[point])), "_month", toString(mon), ".png",sep = "")
    if(mon < 10)
      monStr <- paste("0", toString(mon), month.name[mon],sep = "")
    else
      monStr <- paste(toString(mon), month.name[mon],sep = "")
    
    filename <- paste(pointNames[point], "_", monStr, ".png",sep = "")
    ggsave(paste("img/", filename), plot, width = 12, height = 8, units = "in", type = 'cairo')
    
    plot2 <- ggplot(data = data)+
      geom_line(aes(x=Year, y=Diff2trends, colour="1.Moving average - cumulative average"), size=1.2)+
      geom_line(aes(x=Year, y=DiffTrend30y, colour="2.Moving average - average 1961-1990"), size=1.2)+
      scale_color_npg(name="Variables" )+
      ggtitle(paste("Some differencies '",pointNames[point],"' ",month.name[mon]," for 120 years", sep='') )+
      ylab("Degrees")+
      coord_cartesian(xlim=c(1901, 2022), expand = FALSE)+
      scale_x_continuous(minor_breaks = seq(1901, 2021, 1), breaks = seq(1901, 2021, 10))+
      theme(legend.position="top", legend.text=element_text(size=12), legend.title=element_text(size=10))
    filename <- paste(pointNames[point], "_", monStr, "_diff.png",sep = "")
    ggsave(paste("img/", filename), plot2, width = 12, height = 8, units = "in", type = 'cairo')
    rollMeanYear[,mon] = data$Rollmean
    
  }
  sumYearAvg <- yearAvg[,1] +  yearAvg[,2]+ yearAvg[,3]+ yearAvg[,4]+ yearAvg[,5]+ yearAvg[,6]+
    yearAvg[,7]+ yearAvg[,8]+ yearAvg[,9]+ yearAvg[,10]+ yearAvg[,11]+ yearAvg[,12]
  sumYearAvg = sumYearAvg / 365
  
  #
  grbreaks = 1:13
  grLabels=month.name
  grLabels[13] = "Year average"
  
  #myColorMap <- viridis(12)
  myColorMap <- c("#382985FF", "#9E135CFF", "#F2B6D4FF",
                  "#60969CFF", "#FED168FF", "#81B29AFF",
                  "#C4D76DFF", "#d18d00FF", "#A54A40FF",
                  "#FDC209FF", "#C0DBF4FF", "#3E5C77FF",
                  "#333533FF")
  #myColorMap[13] <- "#000000FF"
  #
  movAvg <- ggplot() + 
    geom_line(aes(x=data$Year, y=rollMeanYear[,1], colour=as.factor(1)), size=1.5)+
    geom_line(aes(x=data$Year, y=rollMeanYear[,2], colour=as.factor(2)), size=1.5)+
    geom_line(aes(x=data$Year, y=rollMeanYear[,3], colour=as.factor(3)), size=1.5)+
    geom_line(aes(x=data$Year, y=rollMeanYear[,4], colour=as.factor(4)), size=1.5)+
    geom_line(aes(x=data$Year, y=rollMeanYear[,5], colour=as.factor(5)), size=1.5)+
    geom_line(aes(x=data$Year, y=rollMeanYear[,6], colour=as.factor(6)), size=1.5)+
    geom_line(aes(x=data$Year, y=rollMeanYear[,7], colour=as.factor(7)), size=1.5)+
    geom_line(aes(x=data$Year, y=rollMeanYear[,8], colour=as.factor(8)), size=1.5)+
    geom_line(aes(x=data$Year, y=rollMeanYear[,9], colour=as.factor(9)), size=1.5)+
    geom_line(aes(x=data$Year, y=rollMeanYear[,10], colour=as.factor(10)), size=1.5)+
    geom_line(aes(x=data$Year, y=rollMeanYear[,11], colour=as.factor(11)), size=1.5)+
    geom_line(aes(x=data$Year, y=rollMeanYear[,12], colour=as.factor(12)), size=1.5)+
    geom_line(aes(x=data$Year, y=sumYearAvg, colour=as.factor(13)), size=1.2)+
    scale_color_manual(values=myColorMap, name="Variables", breaks=grbreaks, labels=grLabels)+
    ggtitle(paste("Moving average (width = 30) by month. Place: '",pointNames[point],"' ", sep='') )+
    ylab("Degrees")+
    xlab("Year")+
    coord_cartesian(xlim=c(1901, 2022), expand = FALSE)+
    scale_x_continuous(minor_breaks = seq(1901, 2021, 1), breaks = seq(1901, 2021, 10))+
    theme(legend.position="top", legend.text=element_text(size=12), legend.title=element_text(size=10))

  filename <- paste(pointNames[point], "_12MovAvg.png",sep = "")
  ggsave(paste("img/", filename), movAvg, width = 12, height = 8, units = "in", type = 'cairo')
}

