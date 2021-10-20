import cv2
import numpy

img = cv2.imread('/Users/yongxiang/Documents/sample/2.jpeg', cv2.IMREAD_COLOR)

img_hsv = cv2.cvtColor(img, cv2.COLOR_BGR2HSV)

mask = cv2.inRange(img_hsv, numpy.array([50, 25, 0]), numpy.array([255, 255, 255]))


contours, hierarchy = cv2.findContours(mask, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE, offset=[0, 0])

for countour in contours:
    x, y, w, h = cv2.boundingRect(countour)

    if h < 25 or w < 25:
        continue

    ratio = w / h

    if ratio <   1:
        continue
    cv2.rectangle(img, (x, y), (x + w, y + h), (0, 255 ,0), 2)



# image_newnew = cv2.drawContours(img, contours, -1, (0, 255, 0), 3)



cv2.imshow('image', img)



cv2.waitKey(0)
