import os
import mimetypes
from flask import Flask
from flask.globals import request
from flask.helpers import send_file
from flask.wrappers import Response
import time
import cv2
import numpy

app = Flask(__name__)

@app.route('/upload', methods=["POST"])
def do_upload():
    upload     = request.files.get('image')
    name, ext = os.path.splitext(upload.filename)
    ts = str(time.time())

    upload.save('/Users/yongxiang/img/'+ts+'.jpeg')
    # upload = open('/Users/yongxiang/img/'+ts+'.jpeg', 'rb')

    ################################################################################################
    img = cv2.imread('/Users/yongxiang/img/'+ts+'.jpeg', cv2.IMREAD_COLOR)

    try:
        img = cv2.rotate(img, cv2.ROTATE_90_CLOCKWISE)

        height, width, channels = img.shape

        img_hsv = cv2.cvtColor(img, cv2.COLOR_BGR2HSV)
        mask = cv2.inRange(img_hsv, numpy.array([50, 25, 0]), numpy.array([255, 255, 255]))

        contours, hierarchy = cv2.findContours(mask, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE, offset=[0, 0])


        for countour in contours:
            x, y, w, h = cv2.boundingRect(countour)

            if h < height / 4 or w < width / 3:
                continue

            ratio = w / h
            if ratio <   1.25:
                continue

            # Record the detected IC
            ic_box = [x, y, w, h]



            break

        ic_box_x, ic_box_y, ic_box_w, ic_box_h = ic_box
        ic_img = img[ic_box_y: (ic_box_y + ic_box_h), ic_box_x: (ic_box_x + ic_box_w)]
        ic_img_hsv = img_hsv[y: (ic_box_y + ic_box_h), x: int((ic_box_x + ic_box_w))]
        ic_img_mask = cv2.inRange(ic_img_hsv, numpy.array([0, 0, 0]), numpy.array([100, 200, 50]))

        ic_img_mask = cv2.erode(ic_img_mask, numpy.ones((1, 2), numpy.uint8))
        ic_img_mask = cv2.dilate(ic_img_mask, numpy.ones((10, 22), numpy.uint8))

        ic_img_contours, ic_img_hierarchy = cv2.findContours(ic_img_mask, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE, offset=[0, 0])

        ic_img_height, ic_img_width, ic_img_channels = ic_img.shape


        i = 0
        childs = [[], [], []]
        for countour in ic_img_contours:
            x, y, w, h = cv2.boundingRect(countour)

            # skip right half ic
            if x + w > ic_img_width / 2:
                continue
            # skip short width
            if (w) < (ic_img_width / 2 * 0.4):
                continue

            # Record Detected IC, Name, Address
            childs[i] = [x, y, w, h]

            i = i + 1
            if i > 3:
                break



        cv2.rectangle(img, (ic_box_x, ic_box_y), (ic_box_x + ic_box_w, ic_box_y + ic_box_h), (0, 255 ,0), 2)

        for child in childs:
            [x, y, w, h] = child

            real_x = ic_box_x + x
            real_y = ic_box_y + y

            child_img = img[real_y: (real_y + h), real_x: (real_x + w)]
            
            child_img = cv2.blur(child_img, (50, 50))

            img[real_y: (real_y + h), real_x: (real_x + w)] = child_img

            cv2.rectangle(img, (real_x, real_y), (real_x + w, real_y + h), (0, 255, 255), 2)


    except:
        print("No detect IC")
        img = cv2.blur(img, (50, 50))
    ################################################################################################################

    img = cv2.rotate(img, cv2.ROTATE_90_COUNTERCLOCKWISE)
    output = cv2.imencode('.jpg', img)[1].tobytes()


    return Response(
        response = output,
        mimetype = 'image/jpeg'
    )

app.run(host='0.0.0.0')