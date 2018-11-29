import json
import os
#import pandas as pd
import matplotlib.pyplot as plt
import numpy as np

inputDir = "C:/Users/princ/OneDrive/Documenti/human-and-robotic-exploration/human-and-robotic-exploration/DownloadedResults/Survey"
fileName = "Result"
index = 1
finish = False

choice = input("Inserire il numero della mappa sulla quale lavorare (1 per uffici1, 2 per open2): ")

if(choice == str(1)):
    map = "uffici1.map"
if(choice == str(2)):
    map = "open2.map"

while(finish == False):
    if os.path.isfile(inputDir + "/" + fileName + str(index) + ".txt"):
        with open(inputDir + "/" + fileName + str(index) + ".txt") as json_file:
            data = json.load(json_file)
            if(data["mapname"] == map):
                choiceone = data["choiceone"]
                choicetwo = data["choicetwo"]
                plt.plot(int(choiceone), int(choicetwo), "ko")
            index = index + 1
            
    else:
        finish = True
   
    
plt.show()






