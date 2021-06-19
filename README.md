

# VicGov Resi Envelope

This function attempts to automate the Victorian Government (AUS) residential planning envelope

|Input Name|Type|Description|
|---|---|---|
|Building facing|string|Building facing condition (declared road or recreation reserve on the other side of the street and opposite the allotment)|
|Front Boundary|https://hypar.io/Schemas/Geometry/Vector3.json|A point which will be used to select the nearest boundary edge|
|Allotment designation|string|Designation of the allotment in the subdivision permit|
|Rear Boundary|https://hypar.io/Schemas/Geometry/Vector3.json|A point which will be used to select the nearest boundary edge|
|Proposed Building Heights|number||


<br>

|Output Name|Type|Description|
|---|---|---|
|Required Setback|Number|The required setback from boundary derived from the proposed building height|

