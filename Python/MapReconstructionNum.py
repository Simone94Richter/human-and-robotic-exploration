import json
import os
#import pandas as pd
import matplotlib.pyplot as plt

inputDir = "C:/Users/princ/OneDrive/Documenti/human-and-robotic-exploration/human-and-robotic-exploration/Unity/Project Arena/Assets/Results"
fileName = "resultMapNum"
fileName2 = "resultPositionNum"
index = 1
keepGoing = True

while(keepGoing == True):
    if os.path.isfile(inputDir + "/" + fileName + str(index) + ".json") and os.path.isfile(inputDir + "/" + fileName2 + str(index) + ".json"):
        with open(inputDir + "/" + fileName + str(index) + ".json") as json_file:
            array_unknown = list()
            array_clear = list()
            array_wall = list()
            array_goal = list()

            if os.path.isfile(inputDir + "/" + fileName + str(index) + ".json"):
                data = json.load(json_file)
                array_unknown = data['u']
                #array_clear = data['r']
                array_wall = data['w']
                array_goal = data['g']
                #print(len(array_unknown))
                for i in range(len(array_unknown)): 
                    for s in array_unknown[i].split():
                        a,b = s.split(",") 
                        a = int(a)
                        b = int(b)
                        #s = int(s)
                        #if s.isdigit():
                        #print(a)
                        #print(b)
                        plt.plot(a, b,'bs')

                for j in range(len(array_wall)):
                    for s in array_wall[j].split():
                        a,b = s.split(",")
                        a = int(a)
                        b = int(b)
                        plt.plot(a, b, 'rs')
            
                for l in range(len(array_goal)):
                    for s in array_goal[l].split():
                        a,b = s.split(",")
                        a = int(a)
                        b = int(b)
                        plt.plot(a, b, 'gs')
        
            #print(len(array_clear))
            #for i in range(len(array_clear)):
            #    for s in array_clear[i].split():
                    #if s.isdigit():
            #            print(s)
            #            plt.plot([], [],'rs')
    
        with open(inputDir + "/" + fileName2 + str(index) + ".json") as json_file:
            data = json.load(json_file)
            array_pos = data['position']
            for k in range(len(array_pos)):
                for s in array_pos[k].split():
                    x,z = s.split(",")
                    x = int(x)
                    z = int(z)
                    #print(x, z)
                    if k+1 < len(array_pos):
                        a,b = array_pos[k+1].split(",")
                        a = int(a)
                        b = int(b)
                        plt.plot([x, a], [z, b], 'k-')

        #plt.axis([0, 50, 0, 50])
        plt.show()
        index = index + 1
    
    else:
        keepGoing = False






