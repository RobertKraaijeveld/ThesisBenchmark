# setting wd
setwd("C:/Projects/Afstudeerexperimenten/Benchmarking Console App/Benchmarking Console App/Output")

library(plotrix)


# Creating all models - visualisation
plot_result_per_db("1542284973_unscaled_simple_drivers_tests.csv",
                   "creating_all_unscaled_visualization.png",
                   "Time spent creating all models per database",
                   "Amount of created models", "Time (ms)", 5000, 
                   "AmountOfModelsInserted", "TimeSpentInsertingModels")

# Getting models by pk - visualisation
plot_result_per_db("1542284973_unscaled_simple_drivers_tests.csv",
                   "getting_by_pk_visualization.png",
                   "Time spent retrieving models (by primary key) per database",
                   "Amount of retrieved models", "Time (ms)", 5000, 
                   "AmountOfModelsRetrievedByPrimaryKey", "TimeSpentRetrievingModelsByPrimaryKey")

# Getting all models
plot_result_per_db("1542284973_unscaled_simple_drivers_tests.csv",
                   "getting_all_unscaled_visualization.png",
                   "Time spent retrieving all models per database",
                   "Amount of retrieved models", "Time (ms)", 5000, 
                   "AmountOfModelsRetrievedByPrimaryKey", "TimeSpentRetrievingAllModels")

# Deleting all models - visualisation
plot_result_per_db("1542284973_unscaled_simple_drivers_tests.csv",
                   "deleting_all_unscaled_visualization.png",
                   "Time spent deleting all models per database",
                   "Amount of deleted models", "Time (ms)", 5000, 
                   "AmountOfModelsRetrievedByPrimaryKey", "TimeSpentDeletingAllModels")
 

# Updating all models - visualisation
plot_result_per_db("1542284973_unscaled_simple_drivers_tests.csv",
                   "updating_all_unscaled_visualization.png",
                   "Time spent updating all models per database",
                   "Amount of updated models", "Time (ms)", 5000, 
                   "AmountOfModelsUpdated", "TimeSpentUpdatingModels")




# Function for transparent colors
t_col <- function(color, percent = 50, name = NULL) {
  #	  color = color name
  #	percent = % transparency
  #	   name = an optional name for the color
  ## Get RGB values for named color
  rgb.val <- col2rgb(color)
  
  return(rgb(rgb.val[1], rgb.val[2], rgb.val[3],
             max = 255,
             alpha = (100-percent)*255/100,
             names = name))
}


plot_result_per_db <- function(filename, outfilename, visualizationtitle, 
                               xaxisName, yAxisName, yAxisMax, 
                               csvColumnToPlotOnXAxis, csvColumnToPlotOnYAxis)
{
  # Creating png with 'cairo' driver so that anti-aliasing is used
  png(outfilename, 800, 800, type='cairo')
  

  #this.dir <- dirname(parent.frame(2)$ofile)
  #setwd(this.dir)
  
  # making lines thicker
  par(lwd=2,cex=1)
  
  # Parse CSV to DF
  TestReport <- data.frame(read.csv(file=filename, header=TRUE, sep=","))
  
  # Using the csvColumnToPlotOnYAxis factor directly as X produces bars,
  # so we only use them as labels.
  csvColumnOnXAxisLabels <- unique(TestReport[,csvColumnToPlotOnXAxis])
  csvColumnOnXAxisLength <- length(unique(TestReport[,csvColumnToPlotOnXAxis]))
  amountOfDbs <- length(unique(TestReport$DatabaseTypeUsed))

  # Preparing colors TODO: NEEDS TO BE DYNAMIC LADS
  colors = c(t_col("black", perc = 15, name = "lt.black"), t_col("blue", perc = 15, name = "lt.blue"),
              t_col("red", perc = 15, name = "lt.red"), t_col("green", perc = 15, name = "lt.green"),
              t_col("pink", perc = 15, name = "lt.pink"), t_col("orange", perc = 15, name = "lt.orange"),
              t_col("yellow", perc = 15, name = "lt.yellow")) 
  
  # Looping through the unique DB types, getting the value for them and 
  # plotting + adding legend.
  counter = 1
  for (dbType in unique(TestReport$DatabaseTypeUsed)) {
    measurementsForThisDb <- subset(TestReport, DatabaseTypeUsed==dbType, select=c(csvColumnToPlotOnYAxis))[, csvColumnToPlotOnYAxis]
    
    if(counter == 1)
    {
      plot(seq(1:csvColumnOnXAxisLength), measurementsForThisDb, 
           ylim=c(0, yAxisMax), pch=19, xaxt="n", yaxt="n",
           col=colors[counter], type="o", cex.axis=1.25, cex.lab=1.5,
           xlab = xaxisName, ylab = yAxisName)
      
      # Setting custom title and axis values
      title(visualizationtitle,cex.main=1.7)
      axis(1, at=1:csvColumnOnXAxisLength, labels=csvColumnOnXAxisLabels, 
           cex.lab=1.5, cex.axis=1.25)
      axis(2, at = seq(0, yAxisMax, by = 5000), 
           cex.lab=1.5, cex.axis=1.25)
      
    }
    else
    {
      lines(seq(1:csvColumnOnXAxisLength), measurementsForThisDb, 
            ylim=c(0, 45000), type = "o", pch=19, 
            col=colors[counter])
   }
   counter <- counter + 1
  }
  
  legend(1, yAxisMax - (yAxisMax / 10), 
         legend=unique(TestReport$DatabaseTypeUsed),
         col=colors, lty=c(1), cex=1.5, pch=19)
  
  # shutting down png drawing driver
  dev.off()
}





