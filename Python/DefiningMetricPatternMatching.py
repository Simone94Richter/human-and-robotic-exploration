import json
import os
#import pandas as pd
import matplotlib.pyplot as plt
import numpy as np
import scipy.spatial
from scipy.cluster.hierarchy import dendrogram, linkage

inputDir = "C:/Users/princ/OneDrive/Documenti/human-and-robotic-exploration/human-and-robotic-exploration/DownloadedResults"
inputDir2 = "C:/Users/princ/OneDrive/Documenti/human-and-robotic-exploration/human-and-robotic-exploration/Python"
fileName = "Result"
fileName2 = ""
index = 1
i = 0
mappa = input("Inserire il numero della mappa sulla quale lavorare (1 per uffici2, 2 per uffici1, 3 per open1 e 4 per open2): ")
finish = False
if(mappa == str(1)):
    map_name = "uffici2.map"
    fileName2 = "uffici2PythonFormat.map.txt"
if(mappa == str(2)):
    map_name = "uffici1.map"
    fileName2 = "uffici1PythonFormat.map.txt"
if(mappa == str(3)):
    map_name = "open1.map"
    fileName2 = "open1PythonFormat.map.txt"
if(mappa == str(4)):
    map_name = "open2.map"
    fileName2 = "open2PythonFormat.map.txt"
print(map_name)

dictionary_path = {}
dictionary_time_path = {}

height_map = 0
width_map = 0
maximum_dist = 0


if os.path.isfile(inputDir2 + "/" + fileName2):
    with open(inputDir2 + "/" + fileName2) as map_file:
        array = []
        content = map_file.readlines()
        content = [x.strip() for x in content]
        height_map = len(content)
        array = content[0].split(',')
        width_map = len(array)
        #print(str(height_map))
        #print(str(width_map))
        #maximum_dist = np.sqrt(height_map * height_map + width_map * width_map)
        #print(str(maximum_dist))


while(finish == False):
    if os.path.isfile(inputDir + "/" + fileName + str(index) + "t.txt"):
        with open(inputDir + "/" + fileName + str(index) + "t.txt") as json_file:
                data = json.load(json_file)
                if(data['mapName'][0] == map_name):
                    array_pos = data['position']
                    dictionary_path[i] = array_pos
                    dictionary_time_path[i] = data['time']
                    print("Numero" + str(i) + ": Result"+ str(index))
                    i = i + 1
                
                index = index + 1
    
    else:
        finish = True

##### Using pattern matching criteria #####
### uncondensed matrix ###

i = 0
len_array = len(dictionary_path)
dist_array = [[0 for x in range(len_array)] for y in range(len_array)]

while(i < len_array):
    path1 = dictionary_path[i]
    j = 0
    while(j < len_array):
        path2 = dictionary_path[j]
        len1 = len(path1) 
        len2 = len(path2)
        
        if (len1 >= len2):
            len_ = len2
            len_max = len1
            path_min = path2 
            path_max = path1
        if (len2 > len1):
            len_ = len1
            len_max = len2
            path_min = path1 
            path_max = path2

        k = 0
        max_concat = 0

        while (k < len_):
            m = 0
            pos1 = path_min[k]

            while(m < len_max):
                count = 0
                l = m
                keepGoing = False
                #print(len_max)
                #print(m)
                pos2 = path_max[m]
                if(pos1 == pos2):
                    count = count + 1
                    keepGoing = True

                while(keepGoing == True and (l + 1) < len_):
                    l = l + 1
                    pos1 = path_min[l]
                    pos2 = path_max[l]
                    if(pos1 != pos2):
                        keepGoing = False
                    else:    
                        count = count + 1
                
                if(count > max_concat):
                    max_concat = count

                m = m + 1

            k = k + 1

        advise = "Simple distance between the path " + str(i) + " and " + str(j) + " :"
        advise_time = "Time of the paths taken in consideration: " + str(dictionary_time_path[i]) + " " + str(dictionary_time_path[j])
        print(advise)
        #print(coord1Array)
        #print(coord2Array)

        print( float(str(float(max_concat/len_max))[:5]) )
        print(advise_time)

        dist_array[i][j] = float(str(float(max_concat/len_max))[:5])
        #dist_array.append(total_dist)

        j = j + 1
    
    i = i + 1 

for x in range(len_array):
    print(dist_array[x])

##### Using pattern matching criteria #####
### condensed matrix ###

i = 0
dist_array = []

while(i < len_array):
    path1 = dictionary_path[i]
    j = i + 1
    while(j < len_array):
        path2 = dictionary_path[j]
        len1 = len(path1) 
        len2 = len(path2)
        
        if (len1 >= len2):
            len_ = len2
            len_max = len1
            path_min = path2 
            path_max = path1
        if (len2 > len1):
            len_ = len1
            len_max = len2
            path_min = path1 
            path_max = path2

        k = 0
        max_concat = 0

        while (k < len_):
            m = 0
            pos1 = path_min[k]

            while(m < len_max):
                count = 0
                l = m
                keepGoing = False
                pos2 = path_max[m]
                if(pos1 == pos2):
                    count = count + 1
                    keepGoing = True

                while(keepGoing == True and (l + 1) < len_):
                    l = l + 1
                    pos1 = path_min[l]
                    pos2 = path_max[l]
                    if(pos1 != pos2):
                        keepGoing = False
                    else:    
                        count = count + 1
                
                if(count > max_concat):
                    max_concat = count

                m = m + 1

            k = k + 1

        #advise = "Simple distance between the path " + str(i) + " and " + str(j) + " :"
        #advise_time = "Time of the paths taken in consideration: " + str(dictionary_time_path[i]) + " " + str(dictionary_time_path[j])
        #print(advise)

        #print( float(str(float(max_concat/len_max))[:5]) )
        #print(advise_time)

        dist_array.append(float(str(float(max_concat/len_max))[:5]))

        j = j + 1
    
    i = i + 1 

for x in range(len(dist_array)):
    print(dist_array[x])

##### clustering part #####

Z = linkage(dist_array, 'ward')
#plt.title('Hierarchical Clustering Dendrogram')
#plt.xlabel('sample index')
#plt.ylabel('distance')
#dendrogram(Z)
#plt.show()