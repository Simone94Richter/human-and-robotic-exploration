import json
import os
#import pandas as pd
import matplotlib.pyplot as plt

inputDir = "C:/Users/princ/OneDrive/Documenti/human-and-robotic-exploration/human-and-robotic-exploration/Python"
fileName = "uffici2PythonFormat.map.txt"
if os.path.isfile(inputDir + "/" + fileName):
    with open(inputDir + "/" + fileName) as f:
        array = []
        content = f.readlines()

        content = [x.strip() for x in content]
        #array_unknown = list()
        #array_clear = list()
        #array_wall = list()
        #array_goal = list()
        j = 0
        for line in content:
            #print(line)
            #array.append(line)
            array = line.split(',')
            #print(line)
            #print(len(array))
            for i in range(len(array)):
                a = int(array[i])
                if(a == 1):
                    plt.plot(i, j, 'ks')
                
                if(a == 4):
                    plt.plot(i, j, 'gs')
                
                if(a == 3):
                    plt.plot(i, j, 'bs')

            j = j + 1    
                
    plt.show()

        






