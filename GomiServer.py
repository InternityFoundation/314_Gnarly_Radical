#Socket Comms 
import time
import zmq

#McQueen Learning
import numpy as np
from keras.preprocessing import image
from keras.models import load_model
from io import BytesIO
#from PIL import Image
#from keras.models import model_from_json
#import json

context = zmq.Context()
socket = context.socket(zmq.REP)
socket.bind("tcp://*:5555")

model = load_model('waste_mod_128.h5')

#with open('model_80%_binary.json','r') as f:
#    model_json = json.load(f)
#    
#model2 = model_from_json(json.dumps(model_json))
#model2.load_weights('model_80%_binary.h5')

while True:
    #Wait for next request from client
    message = socket.recv()
    print("Received request: %s" % message)

    #Do some work
    #image = np.fromstring(message, np.uint8).reshape( 64, 64, 3 )
	#image = cv2.cvtColor(image, cv2.cv.CV_BGR2RGB)
    #test_image = Image.open(BytesIO(message)).resize((64, 64))
    test_image = image.load_img(BytesIO(message), target_size = (128, 128))
    test_image.show()
    test_image = image.img_to_array(test_image)
    test_image = np.expand_dims(test_image, axis = 0)
    result = model.predict(test_image)
    
    time.sleep(0.5)
    
    if result[0][0] == 1:
        socket.send(b'Recyclable')
    else:
        socket.send(b'Organic')

    #Send reply back to client 
    #socket.send(prediction)