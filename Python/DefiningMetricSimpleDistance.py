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

def rotate(x,y, origin=(0,0)):
    # shift to origin
    x1 = x #- origin[0]
    y1 = y #- origin[1]

    #rotate
    x2 = y1
    y2 = -x1

    #if os.path.isfile(inputDir + "/" + fileName + str(i) + "t.txt"):
    #    with open(inputDir + "/" + fileName + str(i) + "t.txt") as json_file:
    #       data = json.load(json_file)
    if(map_name == "uffici2.map"):
                # shift back
        x3 = x2
        y3 = y2 +53
    if(map_name == "open2.map"):
                # shift back
        x3 = x2
        y3 = y2 +57
    if(map_name == "open1.map"):
                # shift back
        x3 = x2
        y3 = y2 +48
    if(map_name == "uffici1.map"):
        x3 = x2
        y3 = y2 +54 

    return x3, y3

if os.path.isfile(inputDir2 + "/" + fileName2):
    with open(inputDir2 + "/" + fileName2) as map_file:
        array = []
        content = map_file.readlines()
        content = [x.strip() for x in content]
        height_map = len(content)
        array = content[0].split(',')
        width_map = len(array)
        print(str(height_map))
        print(str(width_map))
        maximum_dist = np.sqrt(height_map * height_map + width_map * width_map)
        print(str(maximum_dist))


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

##### Using cdist #####
### uncondensed matrix ###

i = 0
c = 0
len_array = len(dictionary_path)
dist_array = [[0 for x in range(len_array)] for y in range(len_array)]

while(i < len_array):
    path1 = dictionary_path[i]
    j = i + 1
    while(j < len_array):
        path2 = dictionary_path[j]
        len1 = len(path1) 
        len2 = len(path2)

        if len1 > len2:
            len_ = len2
            len_max = len1
            path_min = path2 
            path_max = path1
            re_scale_factor = float(len2)/float(len1) 
        else:
            len_ = len1
            len_max = len2
            path_min = path1 
            path_max = path2
            re_scale_factor = float(len1)/float(len2)

        print(str(re_scale_factor))
        #re-scaling both path
        v = 0.0 
        path1_rescaled = []
        path2_rescaled = []
        
        #print(str(v) + ", " + str(w))
        while(v < float(len_)):
            path1_rescaled.append(path_min[int(v)])
            v = float(v) + re_scale_factor

        v = 0.0
        while(v < float(len_max)):
            path2_rescaled.append(path_max[int(v)])
            v = v + 1

        print(len(path1_rescaled))
        print(len(path2_rescaled))
        print(len(path_min))
        print(len(path_max))

        #because sometimes one of the path (rescaled) is longer by one than the other, we add to the shortest a copy of the last element
        if(len(path1_rescaled) > len(path2_rescaled)):
            path2_rescaled.append(path2_rescaled[len(path2_rescaled)-1])

        if(len(path2_rescaled) > len(path1_rescaled)):
            path1_rescaled.append(path1_rescaled[len(path1_rescaled)-1])

        print(len(path1_rescaled))
        print(len(path2_rescaled))
        #print(len(path_min))
        #print(len(path_max))
        k = 0
        #coord1Array = []
        #coord2Array = []
        dist = []

        while (k < len_max):
            pos1 = path1_rescaled[k]
            pos2 = path2_rescaled[k]
            x1, y1 = pos1.split(",")
            x2, y2 = pos2.split(",")
            #coord1.append((float(x1), float(y1)))
            #coord2.append((float(x2), float(y2)))
            #coord1Array.append((float(x1), float(y1))) 
            #coord2Array.append((float(x2), float(y2)))
            coord1 = [(float(x1), float(y1))]
            coord2 = [(float(x2), float(y2))]
            dist.append(scipy.spatial.distance.cdist(coord1, coord2, 'euclidean')[0][0])
            k = k + 1

        #if len_ == len1:
        #    while(k < len2):
        #        pos = path2[k]
        #        x, y = pos.split(",")
        #        coord2Array.append((float(x), float(y)))
        #        dist.append(maximum_dist)
        #        k = k + 1
        #else:
        #    while(k < len1):
        #        pos = path1[k]
        #        x, y = pos.split(",")
        #        coord1Array.append((float(x), float(y)))
        #        dist.append(maximum_dist)
        #        k = k + 1

        advise = "Simple distance between the path " + str(i) + " and " + str(j) + " :"
        advise_time = "Time of the paths taken in consideration: " + str(dictionary_time_path[i]) + " " + str(dictionary_time_path[j])
        print(advise)
        #print(coord1Array)
        #print(coord2Array)

        total_dist = 0
        for num in dist:
            total_dist = total_dist + num

        print(total_dist)
        print(advise_time)

        dist_array[i][j] = total_dist
        #dist_array.append(total_dist)

        #inserire qui il plot dei due path
        plt.subplot(len_array, len_array, c+1)
        with open(inputDir2 + "/" + fileName2) as f:
            array = []
            content = f.readlines()

            content = [x.strip() for x in content]
        
            n = 0
            for line in content:
            
                array = line.split(',')
            
                for m in range(len(array)):
                    a = int(array[m])
                    if(a == 1):
                        plt.plot(m, len(content)-n, 'ws')
                
                    if(a == 4):
                        plt.plot(m, len(content)-n , 'gs')

                n = n + 1    

        for k in range(len(path1)):
            for s in path1[k].split():
                x,z = s.split(",")
                origin = (0.0,0.0)
                x, z = rotate(int(x),int(z), origin )
                    #x = maxlen-int(x)
                    #z = int(z)
                    #print(x, z)
                if k+1 < len(path1):
                    a,b = path1[k+1].split(",")
                    a, b = rotate(int(a),int(b), origin )
                    #a = int(a)
                    #b = int(b)
                    #if(frag == 1):
                    plt.plot([x, a], [z, b], 'k-')
                    
        
        for k in range(len(path2)):
            for s in path2[k].split():
                x,z = s.split(",")
                origin = (0.0,0.0)
                x, z = rotate(int(x),int(z), origin )
                    #x = maxlen-int(x)
                    #z = int(z)
                    #print(x, z)
                if k+1 < len(path2):
                    a,b = path2[k+1].split(",")
                    a, b = rotate(int(a),int(b), origin )
                    #a = int(a)
                    #b = int(b)
                    #if(frag == 1):
                    plt.plot([x, a], [z, b], 'r-')

        plt.title('Distance: ' + str(int(total_dist)), fontsize = 8)
        j = j + 1
        c = c + 1
    
    i = i + 1 

for x in range(len_array):
    print(dist_array[x])

plt.show()

##### Using cdist #####
### condensed matrix ###

#i = 0
#dist_array = []

#while(i < len_array):
#    path1 = dictionary_path[i]
#    j = i + 1
#    while(j < len_array):
#        path2 = dictionary_path[j]
#        len1 = len(path1) 
#        len2 = len(path2)

#        if len1 > len2:
#            len_ = len2
#        else:
#            len_ = len1
        
#        k = 0
#        coord1Array = []
#        coord2Array = []
#        dist = []

#        while (k < len_):
#            pos1 = path1[k]
#            pos2 = path2[k]
#            x1, y1 = pos1.split(",")
#            x2, y2 = pos2.split(",")
            #coord1.append((float(x1), float(y1)))
            #coord2.append((float(x2), float(y2)))
#            coord1Array.append((float(x1), float(y1))) 
#            coord2Array.append((float(x2), float(y2)))
#            coord1 = [(float(x1), float(y1))]
#            coord2 = [(float(x2), float(y2))]
#            dist.append(scipy.spatial.distance.cdist(coord1, coord2, 'euclidean')[0][0])
#            k = k + 1

#        if len_ == len1:
#            while(k < len2):
#                pos = path2[k]
#                x, y = pos.split(",")
#                coord2Array.append((float(x), float(y)))
#                dist.append(maximum_dist)
#                k = k + 1
#        else:
#            while(k < len1):
#                pos = path1[k]
#                x, y = pos.split(",")
#                coord1Array.append((float(x), float(y)))
#                dist.append(maximum_dist)
#                k = k + 1

#        advise = "Simple distance between the path " + str(i) + " and " + str(j) + " :"
#        advise_time = "Time of the paths taken in consideration: " + str(dictionary_time_path[i]) + " " + str(dictionary_time_path[j])
#        print(advise)
        #print(coord1Array)
        #print(coord2Array)

#        total_dist = 0
#        for num in dist:
#            total_dist = total_dist + num

#        print(total_dist)
#        print(advise_time)

        #dist_array[i][j] = total_dist
#        dist_array.append(total_dist)

#        j = j + 1
    
#    i = i + 1 

#for x in range(len(dist_array)):
#    print(dist_array[x])

##### clustering part #####

Z = linkage(dist_array, 'ward')
plt.title('Hierarchical Clustering Dendrogram')
plt.xlabel('sample index')
plt.ylabel('distance')
dendrogram(Z)
plt.show()