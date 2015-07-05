/*
* Author: Qiucw
* DO NOT change order
*/


// -1)
//--------------------------------------------------
function jsb_ReplaceOrPushJsType(jst) {
    //if (!JsTypes) { JsTypes = []; }
    var found = false;
    for (var i = 0; i < JsTypes.length; i++) {
        if (JsTypes[i].fullname == jst.fullname) {
            JsTypes[i] = jst;
            found = true;
            break;
        }
    }
    if (!found) {
        JsTypes.push(jst);
    }
}


// 0) SharpKit library
//--------------------------------------------------
CS.require("SharpKit/jsclr");
CS.require("SharpKit/clrlibrary");

// 1) Files generated by JSBinding
// may overwrite some classes in step 0)
//--------------------------------------------------
CS.require("GeneratedFiles");


// 2) Manually written js
// will overwrite some classes in step 1)
//--------------------------------------------------
CS.require("Manual/UnityEngine_Vector3");
CS.require("Manual/UnityEngine_Vector2");
//
// may be more..
//

// 3) code generated by SharpKit
//--------------------------------------------------
CS.require("SharpKitGeneratedFiles");


// 4) JavaScript implemented
// will overwrite some classes in step 1)
//--------------------------------------------------
CS.require("JSImp/Reflection");
//
// may be more..
//

// 5) SharpKit handler (Compile)
//--------------------------------------------------
CS.require("SharpKit/myclrhandler");

// 6) Error handler
//--------------------------------------------------
CS.require("ErrorHandler");

