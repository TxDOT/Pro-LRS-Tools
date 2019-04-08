# Pro-LRS-Tools
Linear referencing tools for ArcGIS Pro

Copyright 2019 Texas Department of Transportation

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

## Description
This repository contains a Visual Studio solution which creates an ArcGIS Pro add-in tab with two LRS tools which were included in ArcMap but not ArcGIS Pro. 
1) Set From/To Measures
2) Locate Features Along Routes 

![Capture](https://user-images.githubusercontent.com/37301006/55736326-b6764600-59e8-11e9-8ef1-f0bf75329c8c.PNG)


## 1) Set From/To Measures
This tool uses the SetAndInterpolateMsBetween method to assign m-values to the selected m-enabled line. On selection change, the tool will also populate the from and to fields with the minimum and maximum measures of the selected line using the GetMinMaxM method.

### SetAndInterpolateMsBetween
https://github.com/esri/arcgis-pro-sdk/wiki/ProSnippets-Geometry#set-ms-at-the-beginning-and-end-of-the-geometry-and-interpolate-m-values-between-the-two-values-setandinterpolatemsbetween

### GetMinMaxM
https://github.com/esri/arcgis-pro-sdk/wiki/ProSnippets-Geometry#get-the-minimum-and-maximum-m-values-getminmaxm


![Capture](https://user-images.githubusercontent.com/37301006/55737032-16b9b780-59ea-11e9-86e3-4228f97af22c.PNG)


## 2) Locate Features Along Routes
This tool creates a pop-up at the clicked point on a line which returns the Object ID, minimum measure, maximum measure and measure at the point which the user clicked on the line.  The return works on intersections and multiple layers, but can be slow on large datasets and SDE layers.  Edit permission is needed to use this tool because a temporary split is implemented in order to find the measure.  Please note that at this point a redo will appear on the undo/redo stack after running this tool.  Clicking redo after running the tool will cause the split to be redone.

![Capture](https://user-images.githubusercontent.com/37301006/55737427-c98a1580-59ea-11e9-837b-690db0ac984c.PNG)
