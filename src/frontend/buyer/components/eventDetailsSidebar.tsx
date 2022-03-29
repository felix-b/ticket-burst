import React from 'react'
import { Box, Divider, Stat, StatLabel, Tag, TagLabel, Text, VStack } from "@chakra-ui/react"

import { EventSearchFullDetail, EventSearchResult } from '../contracts/backendApi'
import { Utility } from '../utility'
import Link from 'next/link'
import { PriceListBox } from '../components/priceListBox'

export interface EventDetailsSidebarProps {
    fullDetail: EventSearchFullDetail
}

export const EventDetailsSidebar = (props: EventDetailsSidebarProps) => {
    const { fullDetail } = props
    const { event } = fullDetail
    const cardProps = Utility.eventSearchResultToCardProps(event)
    const {
        dateText,
        eventNameText,
        showNameText,
        venueText
    } = cardProps

    const localTimeText = Utility.formatIsoDateTimeAsShortTime(event.eventStartTime)
    const { utcOffsetHours } = event.venueTimeZone
    const utcOffsetHoursText = `GMT ${(utcOffsetHours > 0 ? '+' : '')} ${utcOffsetHours}`
    const priceLevels = Utility.joinPriceLevelsWithPriceList(fullDetail.hall.priceLevels, fullDetail.priceList)

    return (
        <VStack justifyContent='start' textAlign='start' justifyItems='start' gap='20px'>
            <Link href={`/event?id=${event.eventId}`}>
                <Box w='full' cursor='pointer'>
                    <Text fontSize='3xl' fontWeight={400} color='blue.700'>{showNameText}</Text>
                    <Text fontSize='4xl' fontWeight={800} color='blue.700' mt='-12px'>{eventNameText}</Text>
                </Box>
            </Link>

            <Box w='full'>
                <Stat color='blue.700' p='0'>
                    <Divider orientation='horizontal' mb='20px' />

                    <StatLabel fontSize='2xl'>{dateText}</StatLabel>
                    <StatLabel fontSize='1xl' mt='-2px'><big><b>{localTimeText}</b></big> {`${event.venueTimeZone.name} (${utcOffsetHoursText})`}</StatLabel>

                    <StatLabel fontSize='2xl' mt="8px">{venueText}</StatLabel>
                    <StatLabel fontSize='1xl' mt="-2px">{event.venueAddress}</StatLabel>
                    <StatLabel fontSize='1xl' mt="12px">
                        <big>{Utility.formatNumberWithThousandSeparators(fullDetail.hall.totalCapacity)}</big> seats
                        <Tag size={'md'} borderRadius='full' variant='solid' colorScheme='green' ml='10px' verticalAlign='baseline'>
                            <TagLabel fontSize={'md'}>{Utility.formatNumberWithThousandSeparators(fullDetail.event.numberOfSeatsLeft)} left</TagLabel>
                        </Tag>                            
                    </StatLabel>

                    <Divider orientation='horizontal' mt='20px' mb='20px' />

                    <StatLabel fontSize='2xl'>Price List</StatLabel>
                    <PriceListBox priceLevels={priceLevels} />
                </Stat>
            </Box>
        </VStack>
    )
}
