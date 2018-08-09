import json
import os
#import pandas as pd
import matplotlib.pyplot as plt

inputDir = "C:/Users/princ/OneDrive/Documenti/human-and-robotic-exploration/Unity/Project Arena/Assets/Results"
fileName = "resultMap.json"
fileName2 = "resultPosition.json"
if os.path.isfile(inputDir + "/" + fileName) and os.path.isfile(inputDir + "/" + fileName2):
    with open(inputDir + "/" + fileName) as json_file:
        array_unknown = list()
        array_clear = list()
        array_wall = list()
        array_goal = list()

        if os.path.isfile(inputDir + "/" + fileName):
            data = json.load(json_file)
            array_unknown = data['u']
            #array_clear = data['r']
            array_wall = data['w']
            array_goal = data['g']
            #print(len(array_unknown))
            for i in range(len(array_unknown)): 
                for s in array_unknown[i].split():
                    a,b = s.split(",") 
                    #s = int(s)
                    #if s.isdigit():
                    #print(a)
                    #print(b)
                    plt.plot(a, b,'bs')

            for j in range(len(array_wall)):
                for s in array_wall[j].split():
                    a,b = s.split(",")
                    plt.plot(a, b, 'rs')
            
            for l in range(len(array_goal)):
                for s in array_goal[l].split():
                    a,b = s.split(",")
                    plt.plot(a, b, 'gs')
        
            #print(len(array_clear))
            #for i in range(len(array_clear)):
            #    for s in array_clear[i].split():
                    #if s.isdigit():
            #            print(s)
            #            plt.plot([], [],'rs')
    
    with open(inputDir + "/" + fileName2) as json_file:
        data = json.load(json_file)
        array_pos = data['position']
        for k in range(len(array_pos)):
            for s in array_pos[k].split():
                x,z = s.split(",")
                #print(x, z)
                if k+1 < len(array_pos):
                    a,b = array_pos[k+1].split(",")
                    plt.plot([x, a], [z, b], 'k-')

    #plt.axis([0, 50, 0, 50])
    plt.show()






