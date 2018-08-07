import json
import os
#import pandas as pd
import matplotlib.pyplot as plt

inputDir = "C:/Users/princ/OneDrive/Documenti/human-and-robotic-exploration/Unity/Project Arena/Assets/Results"
fileName = "resultMap.json"
if os.path.isfile(inputDir + "/" + fileName):
    with open(inputDir + "/" + fileName) as json_file:
        array_unknown = list()
        array_clear = list()

        if os.path.isfile(inputDir + "/" + fileName):
            data = json.load(json_file)
            array_unknown = data['u']
            array_clear = data['r']
            #print(len(array_unknown))
            for i in range(len(array_unknown)): 
                for s in array_unknown[i].split():
                    a,b = s.split(",") 
                    #s = int(s)
                    #if s.isdigit():
                    #print(a)
                    #print(b)
                    plt.plot(a, b,'bs')
        
            #print(len(array_clear))
            #for i in range(len(array_clear)):
            #    for s in array_clear[i].split():
                    #if s.isdigit():
            #            print(s)
            #            plt.plot([], [],'rs')

        plt.show()