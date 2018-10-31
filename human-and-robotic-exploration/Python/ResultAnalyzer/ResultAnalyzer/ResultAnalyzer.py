import os
import csv
import numpy as np
import seaborn as sns
import matplotlib.pyplot as plt
import matplotlib.lines as lines
import matplotlib.ticker as ticker
import matplotlib.cm as cm
from scipy.stats import wilcoxon, binom_test
from scipy.ndimage.filters import gaussian_filter
from matplotlib.font_manager import FontProperties
import pandas as pd

### FUNCTIONS ###############################################################

# Gets the file name and parses its data.
def getData(inputDir):
    inputAcquired = False

    # fileName = input("\nInsert the file name: ")
    fileName = "data.csv"
    print("\nInsert the file name: " + fileName)

    while inputAcquired == False:
        if os.path.isfile(inputDir + "/" + fileName):
            print("File found.")
            inputAcquired = True
            # Parse the data.
            with open(inputDir + "/" + fileName) as csvfile:
                reader = csv.reader(csvfile, delimiter = ';', quotechar = '|')
                data = list(reader)
        else:
            fileName = input("File not found. Insert the file name: ")
    return data

# Extracts a column from the data.
def getArrayFromData(data, column, conversion=0):
    array = list()

    for i in range(len(data)):
        if (data[i][column] != ""):
            if (conversion == 1):
                try:
                    array.append(int(float(data[i][column])))
                except:
                    pass
            elif (conversion == 2):
                try:
                    array.append(float(data[i][column]))
                except:
                    pass
            else:
                array.append(data[i][column])

    return array

# Counts occurencies in array.
def getCountInData(data, column, map, min, max):
    totalCount = list()

    for i in range(min, max):
        count = 0
        for j in range(len(data)):
            if data[j][1] == map and int(float(data[j][column])) == i:
                count = count + 1
        totalCount.append(count)            

    return totalCount

# Compares the outcomes.
def compareOutcomes(real, perceived, outcome):
    comparison = [0, 0, 0]

    for i in range(len(real)):
        if (real[i] == outcome):
            if (perceived[i] == "safe"):
                comparison[0] = comparison[0] + 1
            elif (perceived[i] == "equal"):
                comparison[1] = comparison[1] + 1
            else:
                comparison[2] = comparison[2] + 1

    return comparison

# Generate the bar diagram of the kills.
def generateBarDiagramKills(data, safe):
    # Extract the data.
    killsSafeArena = getCountInData(data, 6 if safe else 13, "arena", 3, 17)
    killsSafeCorridors = getCountInData(data, 6 if safe else 13, "corridors", 3, 17)
    killsSafeIntense = getCountInData(data, 6 if safe else 13, "intense", 3, 17)
    N = len(killsSafeArena)

    # Setup the graph.
    ind = np.arange(N)
    width = 0.5

    pArena = plt.bar(ind, killsSafeArena, width)
    pCorridors = plt.bar(ind, killsSafeCorridors, width, bottom = killsSafeArena)
    pIntense = plt.bar(ind, killsSafeIntense, width, bottom =  [sum(x) for x in zip(killsSafeArena, killsSafeCorridors)])

    # Plot.
    plt.ylabel('Number of matches')
    plt.xlabel('Kills')
    # plt.title('Kills in maps with ' + ('heuristic' if safe else 'uniform') + ' placement')
    plt.xticks(ind, range(3, 17))
    plt.yticks(np.arange(0, 8, 1))
    ax = plt.subplot(111)
    box = ax.get_position()
    ax.set_position([box.x0, box.y0 + box.height * 0.1,
                 box.width, box.height * 0.9])
    lg = ax.legend((pArena[0], pCorridors[0], pIntense[0]), ('Arena', 'Corridors', 'Intense'),
               loc='upper center', bbox_to_anchor=(0.5, -0.15), ncol = 3)
    lg.draw_frame(False)
    plt.savefig(exportDir + "/" + ("bar_lowrisk" if safe else "bar_uniform"), dpi = 200, bbox_inches = "tight")
    # plt.show()
    plt.clf()

# Generate the bar diagram of the difficulty.
def generateBarDiagramDifficulty(data):
    # Extract the data.
    realDifficulty = getArrayFromData(data, 16)
    perceivedDifficulty = getArrayFromData(data, 17)
    
    safe = compareOutcomes(realDifficulty, perceivedDifficulty, "safe")
    equal = compareOutcomes(realDifficulty, perceivedDifficulty, "equal")
    uniform = compareOutcomes(realDifficulty, perceivedDifficulty, "uniform")

    s = [safe[0], equal[0], uniform[0]]
    e = [safe[1], equal[1], uniform[1]]
    u = [safe[2], equal[2], uniform[2]]

    # Setup the graph.
    N = len(safe)
    ind = np.arange(N)
    width = 0.5

    # Hide the useless axis.
    # ax = plt.subplot(111)
    # ax.plot()
    # ax.spines['right'].set_visible(False)
    # ax.spines['top'].set_visible(False)

    safeBar = plt.bar(ind, s, width)
    equalBar = plt.bar(ind, e, width, bottom = s)
    uniformBar = plt.bar(ind, u, width, bottom = [sum(x) for x in zip(s, e)])

    # Plot.
    plt.xlabel('Placement used in the map with least kills')
    plt.ylabel('Number of test sessions')
    # plt.title('Comparison between the effective and the percived difficulty')
    plt.yticks(np.arange(0, 20, 2))
    plt.xticks(ind, ("Heuristic","No difference","Uniform"))
    ax = plt.subplot(111)
    box = ax.get_position()
    ax.set_position([box.x0, box.y0 + box.height * 0.1,
                 box.width, box.height * 0.9])
    lg = ax.legend((uniformBar[0], equalBar[0], safeBar[0]), ('Uniform placement percived as harder', 
                    'No difference percived', 'Heuristic placement  percived as harder'),
                    loc='upper center', bbox_to_anchor=(0.5, -0.15), ncol = 1)
    lg.draw_frame(False)
    plt.savefig(exportDir + "/percived", dpi = 200, bbox_inches = "tight")
    # plt.show()
    plt.clf()

# Counts the occurencies.
def countOccurencies(array1, array2, i):
    count = 0

    for j in range(len(array1)):
        if array1[i] == array1[j] and array2[i] == array2[j]:
            count = count + 1 

    return count

# Generate scatter diagram.
def generateScatterDiagram(data, column1, column2, showTicks, xlabel, ylabel, title):
    # Extract the data.
    data1 = getArrayFromData(data, column1, 1)
    data2 = getArrayFromData(data, column2, 1)
    maxData = max([max(data1), max(data2)]) + 1
    maxData = maxData + maxData * 0.025
    mean1 = np.mean(data1)
    mean2 = np.mean(data2)
    area = [(np.pi * (30 * countOccurencies(data1, data2, i))) for i in range(len(data1))]

    # Plot.
    fig, ax = plt.subplots()
    plt.ylabel(ylabel)
    plt.xlabel(xlabel)
    # plt.title(title)
    if (showTicks):
        plt.xticks(np.arange(0, maxData, 2))
        plt.yticks(np.arange(0, maxData, 2))
    plt.xlim(0, maxData)
    plt.ylim(0, maxData)
    plt.gca().set_aspect('equal', adjustable='box')
    plt.plot(np.arange(0, maxData, 0.01), np.arange(0, maxData, 0.01), "r--")  
    ax.scatter(data1, data2, area)
    plt.plot(mean1, mean2, "ro")
    plt.savefig(exportDir + "/" + title, dpi = 200, bbox_inches = "tight")
    # plt.show()
    plt.clf()

# Generate scatter diagram of kills.
def generateScatterKills():
    # Extract the data.
    df = pd.read_csv(inputDir + "/datasb.csv", sep = ";", index_col=0)    
    g = sns.JointGrid(x="Kills (heuristic)", y="Kills (uniform)", data=df, xlim = [0, 20], ylim = [0, 20])
    g = g.plot_joint(plt.scatter, color = "m",  alpha=.6)
    _ = g.ax_marg_x.hist(df["Kills (heuristic)"], color = "b", alpha=.6)
    _ = g.ax_marg_y.hist(df["Kills (uniform)"], color = "r", alpha=.6, orientation="horizontal")    
    _ = g.ax_joint.xaxis.set_major_locator(ticker.MultipleLocator(2))
    _ = g.ax_joint.yaxis.set_major_locator(ticker.MultipleLocator(2))
    plt.savefig(exportDir + "/scatter_kills", dpi = 200, bbox_inches = "tight")
    plt.clf()

# Generate scatter diagram of distance.
def generateScatterDistance():
    # Extract the data.
    df = pd.read_csv(inputDir + "/datasb.csv", sep = ";", index_col=0)    
    g = sns.JointGrid(x="Distance (heuristic)", y="Distance (uniform)", data=df, xlim = [400, 800], ylim = [400, 800])
    g = g.plot_joint(plt.scatter, color = "m",  alpha=.6)
    _ = g.ax_marg_x.hist(df["Distance (heuristic)"], color = "b", alpha=.6)
    _ = g.ax_marg_y.hist(df["Distance (uniform)"], color = "r", alpha=.6, orientation="horizontal")    
    _ = g.ax_joint.xaxis.set_major_locator(ticker.MultipleLocator(100))
    _ = g.ax_joint.yaxis.set_major_locator(ticker.MultipleLocator(100))
    plt.savefig(exportDir + "/scatter_distance", dpi = 200, bbox_inches = "tight")
    plt.clf()

# Generate scatter diagram of shots.
def generateScatterShots():
    # Extract the data.
    df = pd.read_csv(inputDir + "/datasb.csv", sep = ";", index_col=0)    
    g = sns.JointGrid(x="Shots (heuristic)", y="Shots (uniform)", data=df, xlim = [0, 300], ylim = [0, 300])
    g = g.plot_joint(plt.scatter, color = "m",  alpha=.6)
    _ = g.ax_marg_x.hist(df["Shots (heuristic)"], color = "b", alpha=.6)
    _ = g.ax_marg_y.hist(df["Shots (uniform)"], color = "r", alpha=.6, orientation="horizontal")    
    _ = g.ax_joint.xaxis.set_major_locator(ticker.MultipleLocator(50))
    _ = g.ax_joint.yaxis.set_major_locator(ticker.MultipleLocator(50))
    plt.savefig(exportDir + "/scatter_shots", dpi = 200, bbox_inches = "tight")
    plt.clf()

# Generate scatter diagram of accuracy.
def generateScatterAccuracy():
    # Extract the data.
    df = pd.read_csv(inputDir + "/datasb.csv", sep = ";", index_col=0)    
    g = sns.JointGrid(x="Accuracy (heuristic)", y="Accuracy (uniform)", data=df, xlim = [0, 1], ylim = [0, 1])
    g = g.plot_joint(plt.scatter, color = "m",  alpha=.6)
    _ = g.ax_marg_x.hist(df["Accuracy (heuristic)"], color = "b", alpha=.6)
    _ = g.ax_marg_y.hist(df["Accuracy (uniform)"], color = "r", alpha=.6, orientation="horizontal")    
    _ = g.ax_joint.xaxis.set_major_locator(ticker.MultipleLocator(0.1))
    _ = g.ax_joint.yaxis.set_major_locator(ticker.MultipleLocator(0.1))
    plt.savefig(exportDir + "/scatter_accuracy", dpi = 200, bbox_inches = "tight")
    plt.clf()

# Generate heatmap diagram of difficulty.
def generateScatterAccuracy():
    # Extract the data.
    df = pd.read_csv(inputDir + "/datasb.csv", sep = ";", index_col=0)    
    g = sns.JointGrid(x="Accuracy (heuristic)", y="Accuracy (uniform)", data=df, xlim = [0, 1], ylim = [0, 1])
    g = g.plot_joint(plt.scatter, color = "m",  alpha=.6)
    _ = g.ax_marg_x.hist(df["Accuracy (heuristic)"], color = "b", alpha=.6)
    _ = g.ax_marg_y.hist(df["Accuracy (uniform)"], color = "r", alpha=.6, orientation="horizontal")    
    _ = g.ax_joint.xaxis.set_major_locator(ticker.MultipleLocator(0.2))
    _ = g.ax_joint.yaxis.set_major_locator(ticker.MultipleLocator(0.2))
    plt.savefig(exportDir + "/scatter_accuracy", dpi = 200, bbox_inches = "tight")
    plt.clf()

def difficultyHeatmap():
    # Extract the data.
    df = pd.DataFrame({'Effective': ['heuristic', 'heuristic', 'heuristic', 'equal', 'equal', 'equal', 'uniform', 'uniform', 'uniform'], 'Perceived': ['heuristic', 'equal', 'uniform', 'heuristic', 'equal', 'uniform', 'heuristic', 'equal', 'uniform'], 'value': [10, 4, 3, 2, 1, 1, 1, 3, 2]})  
    result = df.pivot(index='Effective', columns='Perceived', values='value')
    sns.heatmap(result, annot = True, square = True, cmap = "viridis")
    plt.savefig(exportDir + "/difficulty", dpi = 200, bbox_inches = "tight")
    plt.clf()

def positionHeatmap(dataset):
    # Extract the data.
    with open(inputDir + "/" + dataset) as csvfile:
        reader = csv.reader(csvfile, delimiter = ';', quotechar = '|')
        positions = list(reader)
        x = getArrayFromData(positions, 0, 1)
        y = getArrayFromData(positions, 1, 1)
        heatmap, xedges, yedges = np.histogram2d(x, y, bins = 70)
        heatmap = gaussian_filter(heatmap, sigma = 2)
        plt.imshow(heatmap.T, origin = 'lower', cmap = cm.jet)
        plt.axis('off')
        plt.savefig(exportDir + "/" + dataset.replace('.csv', '') + ".png", bbox_inches='tight')
        plt.clf()

def degree(d, min, max, discardDeadEnd = True):
    if (d == 1 and discardDeadEnd):
        return 0
    else:
        return (1 - (d - min) / (max - min))

def degreeMedium(d, min, max):
    return (1 - abs(0.5 - (d - min) / (max - min)))

# Tells how well a value fits in an interval.
def intervalDistance(min, max, value):
    #return abs(abs(min) - abs(value)) + abs(abs(max) - abs(value))
    if (value >= min and value <= max):
        return 0
    elif (value < min):
        return abs(min - value)
    else:
        return abs(value - max)

# Performs Wicoxon test.
def wilcoxonTest(data, safeColumn, uniformCoulumn):
    safe = getArrayFromData(data, safeColumn, 1)
    uniform = getArrayFromData(data, uniformCoulumn, 1)
    statistic, pvalue = wilcoxon(safe, uniform, 'pratt')
    print("\n[WILCOXON TEST] Results:")
    print("statistics = " + str(statistic))
    print("p-value (two-tiled) = " + str(pvalue))
    print("p-value (one-tiled) = " + str(pvalue / 2))

### MENU FUNCTIONS ############################################################

# Menages the graph menu.
def graphMenu(data):
    index = 0

    while True:
        print("\n[GRAPHS] Select a graph to generate:")
        print("[1] Heuristic bar diagram")
        print("[2] Uniform bar diagram")
        print("[3] Kills")
        print("[4] Distance")
        print("[5] Shots")
        print("[6] Accuracy")
        print("[7] Perception")
        print("[8] Heatmaps")
        print("[0] Back\n")

        option = input("Option: ")
    
        while option != "1" and option != "2" and option != "3" and option != "4" and option != "5" and option != "6" and option != "7" and option != "0":
            option = input("Invalid choice. Option: ")
    
        if option == "1":
            print("\nGenerating graph...") 
            generateBarDiagramKills(data, True)
        elif option == "2":
            print("\nGenerating graph...")  
            generateBarDiagramKills(data, False)
        elif option == "3":
            print("\nGenerating graph...")  
            # generateScatterDiagram(data, 6, 13, True, 'Kills (heuristic)', 
            #                        'Kills (uniform)', 'scatter_kills')
            generateScatterKills()
        elif option == "4":
            print("\nGenerating graph...")
            # generateScatterDiagram(data, 8, 15, False, 'AvgKillDistance (heuristic)', 
            #                        'AvgKillDistance (uniform)', 'scatter_avg')
            generateScatterDistance()
        elif option == "5":
            print("\nGenerating graph...")
            generateScatterShots()
        elif option == "6":
            print("\nGenerating graph...")
            generateScatterAccuracy()
        elif option == "7":
            print("\nGenerating graph...")
            # generateBarDiagramDifficulty(data)
            difficultyHeatmap()
        elif option == "0":
            return

# Menages the function menu.
def functionMenu():
    index = 0

    while True:
        print("\n[FUNCTIONS] Select a function to plot:")
        print("[1] [ROOM] Degree heuristic")
        print("[2] [TILE] Low visibility heuristic")
        print("[3] [TILE] Medium visibility heuristic")
        print("[4] Interval fit")
        print("[0] Back\n")

        option = input("Option: ")
    
        while option != "1" and option != "2" and option != "3" and option != "4" and option != "0":
            option = input("Invalid choice. Option: ")
    
        if option == "1":
            print("\nPlotting function...") 
            t1 = np.arange(0, 16, 1)
            t2 = np.arange(0, 15, 0.001)
            plt.ylabel(r'$D(r)$')
            plt.xlabel(r'$deg(r)$')
            plt.xticks(np.arange(0, 16, 2))
            plt.yticks(np.arange(0, 1.1, 0.2))
            plt.plot(t2, [degree(t, 0, 15) for t in t2], t1, [degree(t, 0, 15) for t in t1], "ro", lw = 2)
            plt.savefig(exportDir + "/degree", dpi = 200, bbox_inches = "tight")
            # plt.show()  
            plt.clf()
        elif option == "2":
            print("\nPlotting function...")
            t1 = np.arange(0, 16, 1)
            t2 = np.arange(0, 15, 0.001)
            plt.ylabel(r'$v(t)$')
            plt.xlabel(r'$deg(t)$')
            plt.xticks(np.arange(0, 16, 2))
            plt.yticks(np.arange(0, 1.1, 0.2))
            plt.plot(t2, [degree(t, 0, 15, False) for t in t2], t1, [degree(t, 0, 15, False) for t in t1], 'ro', lw = 2)
            plt.savefig(exportDir + "/visibility_low", dpi = 200, bbox_inches = "tight")
            # plt.show()
            plt.clf()        
        elif option == "3":
            print("\nPlotting function...")
            t1 = np.arange(0, 16, 1)
            t2 = np.arange(0, 15, 0.001)
            plt.ylabel(r'$v(t)$')
            plt.xlabel(r'$deg(t)$')
            plt.xticks(np.arange(0, 16, 2))
            plt.yticks(np.arange(0, 1.1, 0.2))
            plt.plot(t2, [degreeMedium(t, 0, 15) for t in t2], t1, [degreeMedium(t, 0, 15) for t in t1], 'ro', lw = 2)
            plt.savefig(exportDir + "/visibility_medium", dpi = 200, bbox_inches = "tight")
            # plt.show()
            plt.clf()
        elif option == "4":
            print("\nPlotting function...")
            t1 = np.arange(0, 1.1, 0.1)
            t2 = np.arange(0, 1.02, 0.02)
            plt.xlabel(r'$x$')
            plt.ylabel(r'$d_{int}(x)$')
            plt.xticks(np.arange(0, 1.1, 0.2))
            plt.yticks(np.arange(0, 1.1, 0.2))
            plt.plot(t2, [intervalDistance(0.3, 0.5, t) for t in t2], t1, [intervalDistance(0.3, 0.5, t) for t in t1], 'ro', lw = 2)
            plt.savefig(exportDir + "/interval", dpi = 200, bbox_inches = "tight")
            # plt.show()
            plt.clf()
        elif option == "0":
            return

# Change the font size
def fontMenu():
    userInput = input("\n[FONT] Insert the desired font size: ")

    done = False

    while done is False:
        try:
            val = float(userInput)
            done = True
        except ValueError:
            option = input("Invalid value. Size: ")
    
    font = {'family' : 'serif',
            'serif': ['Computer Modern'],
            'weight' : 'bold',
            'size'   : val}

    plt.rc('text', usetex=True)
    plt.rc('font', **font)

# Menages the heatmap menu.
def heatmapMenu():
    index = 0

    while True:
        print("\n[HEATMAPS] Select a heatmap to generate:")
        print("[1] Arena (heuristic)")
        print("[2] Arena (uniform)")
        print("[3] Corridors (heuristic)")
        print("[4] Corridors (uniform)")
        print("[5] Intense (heuristic)")
        print("[6] Intense (uniform)")
        print("[0] Back\n")

        option = input("Option: ")
    
        while option != "1" and option != "2" and option != "3" and option != "4" and option != "5" and option != "6"  and option != "0":
            option = input("Invalid choice. Option: ")
    
        if option == "1":
            positionHeatmap("heatmap_arena_SS.csv")
        elif option == "2":
            positionHeatmap("heatmap_arena_SUD.csv") 
        elif option == "3":
            positionHeatmap("heatmap_corridors_SS.csv")
        elif option == "4":
            positionHeatmap("heatmap_corridors_SUD.csv")
        elif option == "5":
            positionHeatmap("heatmap_intense_SS.csv")
        elif option == "6":
            positionHeatmap("heatmap_intense_SUD.csv")   
        elif option == "0":
            return

# Menages the Wilcoxon menu.
def wilcoxonMenu(data):
    index = 0

    while True:
        print("\n[WILCOXON TEST] Select the metric to perform the test:")
        print("[1] AvgKillTime")
        print("[2] AvgKillDistance")
        print("[3] Kills")
        print("[4] Distance")
        print("[5] Shots")
        print("[6] Accuracy")
        print("[0] Back\n")

        option = input("Option: ")
    
        while option != "1" and option != "2" and option != "3" and option != "4" and option != "5" and option != "6" and option != "0":
            option = input("Invalid choice. Option: ")
    
        if option == "1":
            wilcoxonTest(data, 7, 14)
        elif option == "2":
            wilcoxonTest(data, 8, 15) 
        elif option == "3":
            wilcoxonTest(data, 6, 13)
        elif option == "4":
            wilcoxonTest(data, 5, 12) 
        elif option == "5":
            wilcoxonTest(data, 2, 9) 
        elif option == "6":
            wilcoxonTest(data, 4, 11) 
        elif option == "0":
            return

### MAIN ######################################################################

# Create the input folder if needed.
inputDir = "./Input"
if not os.path.exists(inputDir):
    os.makedirs(inputDir)

exportDir = "./Export"
if not os.path.exists(exportDir):
    os.makedirs(exportDir)

font = {'family' : 'serif',
        'serif': ['Computer Modern'],
        'weight' : 'bold',
        'size'   : 12.5}

plt.rc('text', usetex=True)
plt.rc('font', **font)

print("RESULT ANALYSIS")

# Get the files and process them.
data = getData(inputDir)

while True:
    print("\n[MENU] Select an option:")
    print("[1] Wilcoxon signed-rank test")
    print("[2] Bernulli validation")
    print("[3] Generate graphs")
    print("[4] Plot functions")
    print("[5] Generate position heatmaps")
    print("[6] Change font size")
    print("[7] Change file")
    print("[0] Quit\n")

    option = input("Option: ")

    while option != "1" and option != "2" and option != "3" and option != "4" and option != "5" and option != "6" and option != "7" and option != "0":
        option = input("Invalid choice. Option: ")
    if option == "1":
        wilcoxonMenu(data);
    elif option == "2":
        harder = getArrayFromData(data, 16)
        safeCount = len([1 for x in harder if x == "safe"])
        uniformCount = len([1 for x in harder if x == "uniform"])
        equalCount = len([1 for x in harder if x == "equal"])
        totalCount = safeCount + uniformCount + equalCount
        pvalue = binom_test(safeCount, totalCount)
        print("\n[BERNULLI TEST] Results:")
        print("#safe = " + str(safeCount))
        print("#uniform = " + str(uniformCount))
        print("#equal = " + str(equalCount))
        print("p-value = " + str(pvalue))
    elif option == "3":
        graphMenu(data)
    elif option == "4":
        functionMenu()
    elif option == "5":
        heatmapMenu()
    elif option == "6":
        fontMenu()
    elif option == "7":
        data = getData(inputDir)
    elif option == "0":
        break
