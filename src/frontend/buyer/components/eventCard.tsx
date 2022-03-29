import React from 'react'
import { Button, Tag, TagLabel, StatGroup, Stat, StatLabel, StatNumber, LinkOverlay } from "@chakra-ui/react"
import { EventCardProps } from '../contracts/props'
import Link from 'next/link'

export const EventCard = (props: EventCardProps) => {
    const {
        eventId,
        dateText,
        eventNameText,
        priceText,
        seatsLeftBadgeText,
        showNameText,
        venueText
    } = props

    return (
        <StatGroup minW='370px' border={'1px solid lightgray'} p={4} borderRadius={'xl'}>
            <Stat color='blue.700'>
                <StatLabel fontSize='1xl'>{dateText}</StatLabel>
                <StatLabel fontSize='1xl' mt="-2px">{venueText}</StatLabel>
                <StatNumber  fontSize='1xl' mt='5px' fontWeight={800}>{showNameText}</StatNumber>
                <StatNumber fontSize='2xl' mt="-10px" fontWeight={800}>{eventNameText}&nbsp;</StatNumber>
                <StatNumber fontSize='md' fontWeight='bold' color='orange.600'>{priceText}</StatNumber>
                <Link href={`/event?id=${eventId}`}>
                    <Button mt='2px' colorScheme='green' fontWeight={'bold'} fontSize='xl' h='40px'>
                        Buy Tickets
                        <Tag size={'sm'} borderRadius='full' variant='solid' colorScheme='orange' ml='10px'>
                            <TagLabel fontSize={'sm'}>{seatsLeftBadgeText}</TagLabel>
                        </Tag>    
                    </Button>
                </Link>
            </Stat>
        </StatGroup>
    )
}
