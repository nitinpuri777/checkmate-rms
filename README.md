# checkmate-rms
RMS VBA application for Checkmate Classic Integrations

##Setting up a new Hotel on Classic
===================================
1. Provision a new "App Password" for the hotel. Use dataworker@checkmate.io for these integrations.
2. You will need the Client Number and Client Password from the Hotel or RMS directly.  [More Details](https://www.dropbox.com/s/tuh9iey91mbcnjt/RMS-Arrivals-%26-Inventory-Reports.pdf?dl=0)
3. RMS will need to authorize Checkmate (AgentID = 44) to make requests to this hotel's endpoint.
4. Download the contents of this repo and open the project with Visual Basic Editor the file 'Console2/Module1.vb' to edit lines 3-12 entering in the values for the constants as appropriate.
5. If you're publishing this application a 2nd time on the same machine, you'll need to:
  * Change Root Namespace to something unique and short (Project > CheckmateExport Properties > Change Assembly Name)
  * Save Module
  * Build -> Publish
  * Change the file path to a new folder (e.g. use the Hotel Name)
  * Finish

6. Setup a Windows Task to run the "CheckmateExports--{ASSEMBLY_NAME}.application" file as frequently as needed.  For Classic this is once daily at 5am property time.
⋅⋅* Run whether user is logged on or not


