using System.Collections;
using System.Collections.Generic;
using UnityEngine;


///<summary> 
///This class produces arrays of Colors that will be used to create the colors of markers for each trial
///</summary>
public class ColorTheme : Manager3D

{
    private const int MaxTrials = 100; //maximum number of trials for each individual

    //purple palette
    private Queue<Color> purplePalette = new Queue<Color>();
    private Color purple1 = new Color32(128, 35, 146, 255); //dark magenta
    private Color purple2 = new Color32(153, 95, 163, 255); //purpureus
    private Color purple3 = new Color32(197, 164, 203, 255); //lilac
    private Color purple4 = new Color32(130, 149, 238, 255); //pewter blue
    private Color purple5 = new Color32(122, 35, 154, 255); //light cyan

    //orange palette
    private Queue<Color> orangePalette = new Queue<Color>();
    private Color orange1 = new Color32(255, 106, 26, 255); //tawny orange
    private Color orange2 = new Color32(226, 113, 29, 255); //dark orange
    private Color orange3 = new Color32(128, 57, 14, 255); //brown
    private Color orange4 = new Color32(255, 182, 39, 255); //honey yellow
    private Color orange5 = new Color32(238, 140, 89, 255); //beige

    //green palette
    private Queue<Color> greenPalette = new Queue<Color>();
    private Color green1 = new Color32(20, 49, 9, 255); //forest green
    private Color green2 = new Color32(90, 152, 84, 255); //sage
    private Color green3 = new Color32(208, 214, 127, 179); //pale green
    private Color green4 = new Color32(15, 96, 23, 179); //pea green
    private Color green5 = new Color32(186, 217, 106, 179); //yellow green

    //pink palette
    private Queue<Color> pinkPalette = new Queue<Color>();
    private Color pink1 = new Color32(239, 81, 89, 255); //salmon
    private Color pink2 = new Color32(243, 131, 117, 255); //orange pink
    private Color pink3 = new Color32(243, 139, 170, 255); //light pink
    private Color pink4 = new Color32(241, 102, 147, 255); //rose
    private Color pink5 = new Color32(183, 112, 130, 255); //mauve

    //blue palette
    private Queue<Color> bluePalette = new Queue<Color>();
    private Color blue1 = new Color32(35, 54, 255, 255); //electric blue
    private Color blue2 = new Color32(2, 8, 135, 255); //dark blue
    private Color blue3 = new Color32(47, 66, 180, 255); //medium blue
    private Color blue4 = new Color32(1, 83, 121, 255); //bright blue
    private Color blue5 = new Color32(68, 136, 185, 255); // light blue


    private Queue<Color>[] trialPalette = new Queue<Color>[MaxTrials]; //a queue of color arrays that will be used to create the colors of markers for each trial

    private Color[] palette = new Color[totalNumMarkers];    //array of colors for a single trial that will be returned in the getColorArray method


    ///<summary> 
    ///This method enqueues all colors on to their respective palettesa
    ///</summary>
    ColorTheme()
    {
        //enqueues all colors on to their respective palettes

        purplePalette.Enqueue(purple1);
        purplePalette.Enqueue(purple2);
        purplePalette.Enqueue(purple3);
        purplePalette.Enqueue(purple4);
        purplePalette.Enqueue(purple5);

        orangePalette.Enqueue(orange1);
        orangePalette.Enqueue(orange2);
        orangePalette.Enqueue(orange3);
        orangePalette.Enqueue(orange4);
        orangePalette.Enqueue(orange5);

        greenPalette.Enqueue(green1);
        greenPalette.Enqueue(green2);
        greenPalette.Enqueue(green3);
        greenPalette.Enqueue(green4);
        greenPalette.Enqueue(green5);

        pinkPalette.Enqueue(pink1);
        pinkPalette.Enqueue(pink2);
        pinkPalette.Enqueue(pink3);
        pinkPalette.Enqueue(pink4);
        pinkPalette.Enqueue(pink5);

        bluePalette.Enqueue(blue1);
        bluePalette.Enqueue(blue2);
        bluePalette.Enqueue(blue3);
        bluePalette.Enqueue(blue4);
        bluePalette.Enqueue(blue5);


        // repeatedly adds each of the five color palette to the trialPalette queue
        for (int i = 0; i < MaxTrials; i += 5)
        {

            trialPalette[i] = purplePalette;
            trialPalette[i + 1] = orangePalette;
            trialPalette[i + 2] = greenPalette;
            trialPalette[i + 3] = pinkPalette;
            trialPalette[i + 4] = bluePalette;
        }

    }

    ///<summary> 
    ///This class returns an array of colors that will be used for the colors of each of the markers in a single trial
    ///</summary>
    /// <param name="trialNum">
    /// number corresponding to the order in which the trial has been created. e.g. for first trial object created, trialNum = 0, for second trial created, trialNum = 1
    /// </param>
    public Color[] getColorArray(int trialNum) //trial num begins at 0
    {
        //initializes a pallette of colors

        for (int i = 0; i < totalNumMarkers; ++i)
        {
            Color temp = trialPalette[trialNum].Dequeue(); //gets a color from the color palette queue corresponding to the trialNum
            palette[i] = temp;
            trialPalette[trialNum].Enqueue(temp);
        }

        return palette;

    }
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }
}
