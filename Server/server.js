const express = require("express");
const socket = require("socket.io");
const cors = require("cors");

// App setup
const port = process.env.PORT || 5000
const app = express();

app.use(cors());

const server = app.listen(port, function () {
  console.log(`Listening on port ${port}`);
});

// Socket setup
const io = require("socket.io")(server, {
  cors:
  {
    "origin": "https://data.virtualbrainlab.org",
    "methods": "GET,HEAD,PUT,PATCH,POST,DELETE",
    "preflightContinue": false,
    "optionsSuccessStatus": 204
  } 
});

ID2Socket = {}; // keeps track of all sockets with the same ID
Socket2ID = {}; // keeps track of the ID of each socket
Socket2Type = {};

io.on("connection", function (socket) {
  console.log("Client connected with ID: " + socket.id);

  socket.on('disconnect', () => {
  	console.log('Client disconnected with ID: ' + socket.id);

  	if (ID2Socket[Socket2ID[socket.id]]) {
      ID2Socket[Socket2ID[socket.id]].splice(ID2Socket[Socket2ID[socket.id]].indexOf(socket.id),1);
      Socket2ID[socket.id] = undefined;
  	}
  })

  socket.on('ID', function(clientData) {
    // ID is just a unique identifier, it can be any string
    newClientID = clientData[0]
    // Type can be "send" or "receive"
    newClientType = clientData[1]


  	console.log('Client ' + socket.id + ' requested to update ID to ' + newClientID);
  	// Check if we have an old clientID that needs to be removed
  	if (Socket2ID[socket.id]) {
  		oldClientID = Socket2ID[socket.id]

  		ID2Socket[oldClientID].splice(ID2Socket[oldClientID].indexOf(socket.id),1);
  	}

  	if (ID2Socket[newClientID] == undefined) {
  		// save the new entry into a new list
  		ID2Socket[newClientID] = [socket.id];
  	}
  	else {
  		// save the new entry
  		ID2Socket[newClientID].push(socket.id);
  	}
  	// update the client ID locally
	  Socket2ID[socket.id] = newClientID;
    Socket2Type[socket.id] = newClientType
  	console.log('User updated their ID to: ' + Socket2ID[socket.id] + " type " + newClientType );
  	console.log('All connected clients with ID: ' + ID2Socket[Socket2ID[socket.id]]);
  });

  // The following is simply a list of all events that can be sent by a client and need to be re-emitted to the receivers
  // no actual functionality is implemented by the echo server
  
  // CCF Areas
  socket.on('LoadDefaultAreas', function(data) {
    emitToReceiver(socket.id, 'LoadDefaultAreas', data);
  });

  socket.on('SetAreaColors', function(data) {
  	emitToReceiver(socket.id, 'SetAreaColors', data);
  });
  socket.on('SetAreaVisibility', function(data) {
  	emitToReceiver(socket.id, 'SetAreaVisibility', data);
  });
  socket.on('SetAreaIntensity', function(data) {
    emitToReceiver(socket.id, 'SetAreaIntensity', data);
  });
  socket.on('SetAreaColormap', function(data) {
    emitToReceiver(socket.id, 'SetAreaColormap', data);
  });
  socket.on('SetAreaMaterial', function(data) {
    emitToReceiver(socket.id, 'SetAreaMaterial', data);
  });
  socket.on('SetAreaAlpha', function(data) {
    emitToReceiver(socket.id, 'SetAreaAlpha', data);
  });
  socket.on('SetAreaData', function(data) {
    emitToReceiver(socket.id, 'SetAreaData', data);
  });
  socket.on('SetAreaIndex', function(data) {
    emitToReceiver(socket.id, 'SetAreaIndex', data);
  });
  
  // Neurons
  socket.on('CreateNeurons', function(data) {
    emitToReceiver(socket.id, 'CreateNeurons', data);
  });
  socket.on('DeleteNeurons', function(data) {
    emitToReceiver(socket.id, 'DeleteNeurons', data);
  });
  socket.on('SetNeuronPos', function(data) {
    emitToReceiver(socket.id, 'SetNeuronPos', data);
  });
  socket.on('SetNeuronSize', function(data) {
    emitToReceiver(socket.id, 'SetNeuronSize', data);
  });
  socket.on('SetNeuronShape', function(data) {
    emitToReceiver(socket.id, 'SetNeuronShape', data);
  });
  socket.on('SetNeuronColor', function(data) {
    emitToReceiver(socket.id, 'SetNeuronColor', data);
  });
  socket.on('SetNeuronMaterial', function(data) {
    emitToReceiver(socket.id, 'SetNeuronMaterial', data);
  });

  // Probes
  socket.on('CreateProbes', function(data) {
    emitToReceiver(socket.id, 'CreateProbes', data);
  });
  socket.on('DeleteProbes', function(data) {
    emitToReceiver(socket.id, 'DeleteProbes', data);
  });
  socket.on('SetProbeColors', function(data) {
    emitToReceiver(socket.id, 'SetProbeColors', data);
  });
  socket.on('SetProbePos', function(data) {
    emitToReceiver(socket.id, 'SetProbePos', data);
  });
  socket.on('SetProbeAngles', function(data) {
    emitToReceiver(socket.id, 'SetProbeAngles', data);
  });
  socket.on('SetProbeStyle', function(data) {
    emitToReceiver(socket.id, 'SetProbeStyle', data);
  });
  socket.on('SetProbeSize', function(data) {
    emitToReceiver(socket.id, 'SetProbeSize', data);
  });

  // Volumes
  socket.on('CreateVolume', function(data) {
    emitToReceiver(socket.id, 'CreateVolume', data);
  });
  socket.on('DeleteVolume', function(data) {
    emitToReceiver(socket.id, 'DeleteVolume', data);
  });
  socket.on('SetVolumeVisibility', function(data) {
    emitToReceiver(socket.id, 'SetVolumeVisibility', data);
  });
  socket.on('SetVolumeData', function(data) {
    emitToReceiver(socket.id, 'SetVolumeData', data);
  });
  socket.on('SetVolumeDataMeta', function(data) {
    emitToReceiver(socket.id, 'SetVolumeDataMeta', data);
  });
  socket.on('SetVolumeColormap', function(data) {
    emitToReceiver(socket.id, 'SetVolumeColormap', data);
  });

  // Camera
  socket.on('SetCameraTarget', function(data) {
    emitToReceiver(socket.id, 'SetCameraTarget', data);
  });
  socket.on('SetCameraPosition', function(data) {
    emitToReceiver(socket.id, 'SetCameraPosition', data);
  });
  socket.on('SetCameraRotation', function(data) {
    emitToReceiver(socket.id, 'SetCameraRotation', data);
  });
  socket.on('SetCameraTargetArea', function(data) {
    emitToReceiver(socket.id, 'SetCameraTargetArea', data);
  });
  socket.on('SetCameraZoom', function(data) {
    emitToReceiver(socket.id, 'SetCameraZoom', data);
  });
  socket.on('SetCameraPan', function(data) {
    emitToReceiver(socket.id, 'SetCameraPan', data);
  });

  // Receiver events
  socket.on('log', function(data) {
    emitToSender(socket.id, 'log', data);
  });
  socket.on('warning', function(data) {
    emitToSender(socket.id, 'warning', data);
  });
  socket.on('error', function(data) {
    emitToSender(socket.id, 'error', data);
  });

  // Text
  
  socket.on('CreateText', function(data) {
    emitToReceiver(socket.id, 'CreateText', data);
  });
  socket.on('DeleteText', function(data) {
    emitToReceiver(socket.id, 'DeleteText', data);
  });
  socket.on('SetTextText', function(data) {
    emitToReceiver(socket.id, 'SetTextText', data);
  });
  socket.on('SetTextColors', function(data) {
    emitToReceiver(socket.id, 'SetTextColors', data);
  });
  socket.on('SetTextSizes', function(data) {
    emitToReceiver(socket.id, 'SetTextSizes', data);
  });
  socket.on('SetTextPositions', function(data) {
    emitToReceiver(socket.id, 'SetTextPositions', data);
  });

  // Line Renderer

  socket.on('CreateLine', function(data) {
    emitToReceiver(socket.id, 'CreateLine', data);
  });
  socket.on('DeleteLine', function(data) {
    emitToReceiver(socket.id, 'DeleteLine', data);
  });
  socket.on('SetLinePosition', function(data) {
    emitToReceiver(socket.id, 'SetLinePosition', data);
  });
  socket.on('SetLineColor', function(data) {
    emitToReceiver(socket.id, 'SetLineColor', data);
  });

  // Primitive Mesh Renderer
  socket.on('CreateMesh', function(data) {
    emitToReceiver(socket.id, 'CreateMesh', data);
  });
  socket.on('DeleteMesh', function(data) {
    emitToReceiver(socket.id, 'DeleteMesh', data);
  });
  socket.on('SetPosition', function(data) {
    emitToReceiver(socket.id, 'SetPosition', data);
  });
  socket.on('SetScale', function(data) {
    emitToReceiver(socket.id, 'SetScale', data);
  });
  socket.on('SetColor', function(data) {
    emitToReceiver(socket.id, 'SetColor', data);
  });
  
  // Clear
  socket.on('Clear', function(data) {
    emitToReceiver(socket.id, 'Clear', data);
  });
});

function emitToReceiver(id, event, data) {
  	console.log('Sender sent event: ' + event + ' emitting to all clients with ID: ' + Socket2ID[id] + " and type receive");
  	for (var socketID of ID2Socket[Socket2ID[id]]) {
      if (Socket2Type[socketID]=="receive") {
        console.log('Emitting to: ' + socketID);
        io.to(socketID).emit(event,data);
      }
  	}
}

function emitToSender(id, event, data) {
  console.log('Receiver sent event: ' + event + ' emitting to all clients with ID: ' + Socket2ID[id] + " and type send");
  for (var socketID of ID2Socket[Socket2ID[id]]) {
    if (Socket2Type[socketID]=="send") {
      console.log('Emitting to: ' + socketID);
      io.to(socketID).emit(event,data);
    }
  }
}